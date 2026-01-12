using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;
using FileParserService.FileSystem;
using Microsoft.Extensions.Logging;
using Shared.Models.Parser.XML;

namespace FileParserService.Parsing;

public class XmlParser
{
    private readonly ILogger<XmlParser> _logger;
    private readonly XmlFileProvider _fileProvider;
    private readonly FolderPathService _folderPathService;
    private readonly TimeSpan _pollIntervalMs;

    private static readonly XmlSerializer InstrumentSerializer = new(typeof(InstrumentStatus));

    public XmlParser(
        ILogger<XmlParser> logger,
        FolderPathService folderPathService,
        XmlFileProvider fileProvider,
        int pollIntervalMs = 1000)
    {
        _logger = logger;
        _folderPathService = folderPathService;
        _fileProvider = fileProvider;
        _pollIntervalMs = TimeSpan.FromMilliseconds(pollIntervalMs);
    }

    public async IAsyncEnumerable<InstrumentStatus> StartParseAsync([EnumeratorCancellation] CancellationToken token)
    {
        _logger.LogInformation("Начало мониторинга директории.");

        var folderPath = _folderPathService.GetFolderPath();
        
        _fileProvider.SetRootPath(folderPath);
        _fileProvider.EnsureDirectories();

        while (!token.IsCancellationRequested)
        {
            if (!_fileProvider.GetXmlFiles(out var files))
            {
                _logger.LogError("Ошибка при получении файлов из директории.");
                await Task.Delay(_pollIntervalMs, token);
                continue;
            }

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                InstrumentStatus? model = null;
                var processedSuccessfully = false;

                try
                {
                    await using var stream = _fileProvider.OpenRead(filePath);
                    
                        var settings = new XmlReaderSettings
                        {
                            DtdProcessing = DtdProcessing.Prohibit,
                            IgnoreComments = true,
                            IgnoreWhitespace = true
                        };

                        using var xmlReader = XmlReader.Create(stream, settings);

                        model = (InstrumentStatus?)InstrumentSerializer.Deserialize(xmlReader);

                        if (model == null)
                            throw new InvalidOperationException("Десериализация вернула null");
                    


                    if (!model.DeviceStatus.Any())
                        throw new InvalidOperationException("Файл не содержит данных об устройствах");

                    _fileProvider.MoveToProcessed(filePath);

                    processedSuccessfully = true;

                    _logger.LogInformation("Файл {FileName} успешно десериализован.", fileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке файла: {FileName}", fileName);

                    try
                    {
                        var errorPath = _fileProvider.MoveToError(filePath);
                        _logger.LogWarning("Файл перемещён в папку {ErrorPath}.", errorPath);
                    }
                    catch (Exception moveEx)
                    {
                        _logger.LogError(moveEx,
                            "Не удалось переместить файл {FilePath} в Error", filePath);
                    }
                }

                if (processedSuccessfully && model != null)
                {
                    yield return model;
                }
            }
            
            await Task.Delay(_pollIntervalMs, token);
        }

        _logger.LogInformation("Мониторинг директории завершён.");
    }
}