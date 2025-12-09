using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FileParserService.Service;

public class FolderPathService(IConfiguration config, ILogger logger, string configPath)
{
    public async Task<string> GetFolderPathAsync()
    {
        string? folderPath = config["FolderPath"];
        
        logger.LogInformation("Чтение пути из конфигурации.");

        if (string.IsNullOrWhiteSpace(folderPath))
            logger.LogWarning("Путь к папке в конфигурации отсутствует");
        else if (!Directory.Exists(folderPath))
            logger.LogWarning("Путь в конфигурации найден, но директория не существует.");

        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            Console.WriteLine("Путь отсутствует или директория не существует.");
            Console.Write("Введите путь к директории: ");
            
            folderPath = Console.ReadLine();
            logger.LogInformation("Путь введенный пользователем: " + folderPath);
            
            while (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                logger.LogWarning("Пользователь ввёл некорректный путь: " + folderPath);
                Console.WriteLine("Такой директории не существует. Повторите ввод.");
                Console.Write("Введите путь: ");
                folderPath = Console.ReadLine();
            }
            
            await UpdateJsonConfigAsync(folderPath);
            logger.LogInformation("Путь обновлен и сохранен.");
        }
        else
        {
            logger.LogInformation("Путь успешно загружен.");
        }
        
        return folderPath;
    }

    private async Task UpdateJsonConfigAsync(string folderPath)
    {
        logger.LogInformation("Обновление файла конфигурациию.");
        
        var json = await File.ReadAllTextAsync(configPath);
        var data = JObject.Parse(json);
        
        data["FolderPath"] = folderPath;
        
        await File.WriteAllTextAsync(
            configPath, 
            data.ToString(Formatting.Indented));
        
        logger.LogInformation("Файл конфигурации обновлен.");
    }
}