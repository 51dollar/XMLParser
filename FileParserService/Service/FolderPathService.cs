using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FileParserService.Service;

public class FolderPathService
{
    private readonly IConfiguration _config;
    private readonly ILogger<FolderPathService> _logger;
    private readonly string _configPath;

    public FolderPathService(IConfiguration config, ILogger<FolderPathService> logger, string configPath)
    {
        _config = config;
        _logger = logger;
        _configPath = configPath;
    }

    public async Task<string> GetFolderPathAsync()
    {
        string? folderPath = _config["FolderPath"];
        _logger.LogInformation("Чтение пути из конфигурации.");

        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            _logger.LogWarning("Путь отсутствует или директория не существует.");
            folderPath = PromptUserForFolderPath();
            await UpdateJsonConfigAsync(folderPath);
            _logger.LogInformation("Путь обновлён и сохранён в конфигурации.");
        }
        else
        {
            _logger.LogInformation("Путь успешно загружен: {FolderPath}", folderPath);
        }

        return folderPath;
    }

    private string PromptUserForFolderPath()
    {
        string? folderPath;
        do
        {
            Console.Write("Введите путь к директории: ");
            folderPath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                _logger.LogWarning("Некорректный путь: {FolderPath}", folderPath);
                Console.WriteLine("Такой директории не существует. Повторите ввод.");
            }
        } while (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath));

        _logger.LogInformation("Путь, введённый пользователем: {FolderPath}", folderPath);
        return folderPath;
    }

    private async Task UpdateJsonConfigAsync(string folderPath)
    {
        _logger.LogInformation("Обновление файла конфигурации: {ConfigPath}", _configPath);

        var json = await File.ReadAllTextAsync(_configPath);
        var data = JObject.Parse(json);
        data["FolderPath"] = folderPath;

        await File.WriteAllTextAsync(_configPath, data.ToString(Formatting.Indented));
        _logger.LogInformation("Файл конфигурации успешно обновлён.");
    }
}