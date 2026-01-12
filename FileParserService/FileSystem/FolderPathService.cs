using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileParserService.FileSystem;

public class FolderPathService(
    IConfiguration configuration,
    ILogger<FolderPathService> logger)
{
    public string GetFolderPath()
    {
        var folderPath = configuration.GetRequiredSection("FolderPath").Value;

        if (string.IsNullOrWhiteSpace(folderPath))
            throw new InvalidOperationException("FolderPath не задан appsettings.json");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            logger.LogInformation(
                "Директория {FolderPath} создана", folderPath);
        }

        logger.LogInformation(
            "Используется директория XML: {FolderPath}", folderPath);

        return folderPath;
    }
}