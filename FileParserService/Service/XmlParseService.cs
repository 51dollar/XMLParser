using System.Xml.Serialization;
using FileParserService.Models;
using Microsoft.Extensions.Logging;

namespace FileParserService.Service;

public class XmlParseService(string folderPath, ILogger logger)
{
    private int _interval = 1000;

    public async IAsyncEnumerable<ModelXmlParse> StartParse(CancellationToken token)
    {
        logger.LogInformation("Запуск мониторинга директории по пути: " + folderPath);
        
        Directory.CreateDirectory(Path.Combine(folderPath, "Processed"));
        Directory.CreateDirectory(Path.Combine(folderPath, "Error"));
        
        logger.LogInformation("Созданы директории для обработки.");

        while (!token.IsCancellationRequested)
        {
            var files = Directory.GetFiles(folderPath, "*.xml");
            foreach (var filePath in files)
            {
                ModelXmlParse model = null;
                try
                {
                    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    XmlSerializer serializer = new XmlSerializer(typeof(ModelXmlParse));
                    model = (ModelXmlParse)serializer.Deserialize(stream);
                    logger.LogInformation("Файл успешно десериализован: " + filePath);
                    
                    string processedFolderPath = Path.Combine(folderPath, "Processed", Path.GetFileName(filePath));
                    File.Move(filePath, processedFolderPath, overwrite: true);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Ошибка при обработке файла {filePath}: "  + ex.Message);
                    
                    try
                    {
                        string errorFolderPath = Path.Combine(folderPath, "Error", Path.GetFileName(filePath));
                        File.Move(filePath, errorFolderPath, overwrite: true);
                    }
                    catch (Exception moveEx)
                    {
                        logger.LogError(moveEx, $"Не удалось переместить файл {filePath} в Error");
                    }
                }

                if (model != null)
                    yield return model;
            }

            await Task.Delay(_interval, token);
        }
    }
}