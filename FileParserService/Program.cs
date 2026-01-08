using FileParserService.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
        });

        services.AddSingleton<ProcessedModelService>();
        services.AddSingleton<FolderPathService>(provider =>
        {
            var configuration = context.Configuration;
            var logger = provider.GetRequiredService<ILogger<FolderPathService>>();
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            return new FolderPathService(configuration, logger, configPath);
        });
        services.AddSingleton<StatusChangeService>();
        services.AddSingleton<JsonParserService>();
        services.AddSingleton<RabbitService>();
        services.AddHostedService(provider => 
            provider.GetRequiredService<RabbitService>());
        services.AddHostedService<XmlParseWorker>();
        services.AddHostedService<JsonPublishWorker>();
    })
    .Build();

await host.RunAsync();