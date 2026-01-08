using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Shared.Models.Parser.XML;

namespace FileParserService.Service;

public class XmlParseService
{
    private readonly string _folderPath;
    private readonly ILogger<XmlParseService> _logger;
    private readonly TimeSpan _pollIntervalMs;
    
    private static readonly XmlSerializer InstrumentSerializer = new(typeof(InstrumentStatus));

    public XmlParseService(
        string folderPath, 
        ILogger<XmlParseService> logger,
        int pollIntervalMs = 1000)
    {
        _folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pollIntervalMs = TimeSpan.FromMilliseconds(pollIntervalMs);
        
        EnsureDirectories();
    }
    
    private void EnsureDirectories()
    {
        Directory.CreateDirectory(Path.Combine(_folderPath, "Processed"));
        Directory.CreateDirectory(Path.Combine(_folderPath, "Error"));

        _logger.LogInformation("Директории Processed и Error готовы.");
    }

    public async IAsyncEnumerable<InstrumentStatus> StartParse([EnumeratorCancellation] CancellationToken token)
    {
        _logger.LogInformation("Начало мониторинга директории.");

        while (!token.IsCancellationRequested)
        {
            string[] files;
            try
            {
                files = Directory.GetFiles(_folderPath, "*.xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении файлов из директории: {Message}", ex.Message);
                await Task.Delay(_pollIntervalMs, token);
                continue;
            }

            foreach (var filePath in files)
            {
                InstrumentStatus? model = null;
                var processedSuccessfully = false;
                try
                {
                    await using (var stream = new FileStream(
                        filePath, 
                        FileMode.Open, 
                        FileAccess.Read,
                        FileShare.ReadWrite))
                    {
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
                    }
                    
                    
                    if (!model.DeviceStatus.Any())
                        throw new InvalidOperationException("Файл не содержит данных об устройствах");
                        
                    var processedPath = Path.Combine(_folderPath, "Processed", Path.GetFileName(filePath));
                    File.Move(filePath, processedPath, overwrite: true);
                    
                    processedSuccessfully = true;

                    _logger.LogInformation("Файл {FileName} успешно десериализован и перемещён. PackageId: {PackageId}",
                        Path.GetFileName(filePath), model.PackageId);
                    _logger.LogDebug("Файл перемещён в: {ProcessedPath}", processedPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке файла: {ex}", ex.Message);

                    try
                    {
                        var errorPath = Path.Combine(_folderPath, "Error", Path.GetFileName(filePath));
                        File.Move(filePath, errorPath, overwrite: true);
                        _logger.LogWarning("Файл перемещён в папку {ErrorPath}.", errorPath);
                    }
                    catch (Exception moveEx)
                    {
                        _logger.LogError(moveEx,
                            "Не удалось переместить файл {FilePath} в Error: {Message}", filePath,moveEx.Message);
                    }
                }

                if (processedSuccessfully && model != null)
                {
                    yield return model;
                }
            }

            try
            {
                await Task.Delay(_pollIntervalMs, token);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Пользователь завершил мониторинг директории!");
            }
        }

        _logger.LogInformation("Мониторинг директории завершён.");
    }
}