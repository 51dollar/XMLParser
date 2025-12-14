using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace FileParserService.Service;

public class XmlParseService
{
    private readonly string _folderPath;
    private readonly ILogger<XmlParseService> _logger;
    private readonly TimeSpan _pollIntervalMs;

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

    public async IAsyncEnumerable<ModelXmlParse> StartParse([EnumeratorCancellation] CancellationToken token)
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
                _logger.LogError(ex, "Ошибка при получении файлов из директории:" +  ex.Message);
                await Task.Delay(_pollIntervalMs, token);
                continue;
            }

            foreach (var filePath in files)
            {
                ModelXmlParse model = null;
                try
                {
                    await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var serializer = new XmlSerializer(typeof(ModelXmlParse));
                    model = (ModelXmlParse)serializer.Deserialize(stream);
                    _logger.LogInformation("Файл успешно десериализован.");
                    
                    var processedPath = Path.Combine(_folderPath, "Processed", Path.GetFileName(filePath));
                    File.Move(filePath, processedPath, overwrite: true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке файла: " + ex.Message);

                    try
                    {
                        var errorPath = Path.Combine(_folderPath, "Error", Path.GetFileName(filePath));
                        File.Move(filePath, errorPath, overwrite: true);
                        _logger.LogWarning("Файл перемещён в папку Error.");
                    }
                    catch (Exception moveEx)
                    {
                        _logger.LogError(moveEx, "Не удалось переместить файл в Error." + filePath);
                    }
                }

                if (model != null)
                    yield return model;
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