using Microsoft.Extensions.Logging;

namespace FileParserService.FileSystem;

public class XmlFileProvider(ILogger<XmlFileProvider> logger)
{
    private string? _rootPath;
    
    public void SetRootPath(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Путь к директории не задан", nameof(folderPath));

        _rootPath = folderPath;
        
        logger.LogInformation("Корневая директория XML установлена: {FolderPath}", _rootPath);
    }
    
    public void EnsureDirectories()
    {
        EnsureRootPath();
            
        Directory.CreateDirectory(Path.Combine(_rootPath!, "Processed"));
        Directory.CreateDirectory(Path.Combine(_rootPath!, "Error"));
        
        logger.LogInformation("Директории Processed и Error проверены/созданы");
    }

    public bool GetXmlFiles(out string[] files)
    {
        EnsureRootPath();

        try
        {
            files = Directory.GetFiles(_rootPath!, "*.xml");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при чтении XML файлов из директории {FolderPath}", _rootPath);

            files = Array.Empty<string>();
            return false;
        }
    }

    public Stream OpenRead(string filePath)
    {
        return new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite);
    }
    public void MoveToProcessed(string filePath)
    {
        EnsureRootPath();
        
        var target = Path.Combine(_rootPath!, "Processed", Path.GetFileName(filePath));
        File.Move(filePath, target, overwrite: true);
    }

    public string MoveToError(string filePath)
    {
        EnsureRootPath();
        
        var target = Path.Combine(_rootPath!, "Error", Path.GetFileName(filePath));
        File.Move(filePath, target, overwrite: true);
        return target;
    }
    
    private void EnsureRootPath()
    {
        if (_rootPath == null)
            throw new InvalidOperationException(
                "RootPath не установлен. Вызови SetRootPath перед использованием XmlFileProvider.");
    }
}