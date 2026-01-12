using FileParserService.FileSystem;
using FileParserService.Messaging;
using FileParserService.Parsing;
using FileParserService.Processing;
using FileParserService.Service;
using FileParserService.Workers;
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

        services.AddSingleton<ProcessedModelHandler>();
        services.AddSingleton<FolderPathService>();
        services.AddSingleton<XmlFileProvider>();
        services.AddSingleton<ModuleStateGenerator>();
        services.AddSingleton<StatusChangeProcessor>();
        services.AddSingleton<JsonMessageSerializer>();
        services.AddSingleton<RabbitPublisher>();
        services.AddHostedService(provider => 
            provider.GetRequiredService<RabbitPublisher>());
        services.AddSingleton<XmlParser>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<XmlParser>>();
            var folderPathService = provider.GetRequiredService<FolderPathService>();
            var fileProvider = provider.GetRequiredService<XmlFileProvider>();

            return new XmlParser(
                logger,
                folderPathService,
                fileProvider,
                pollIntervalMs: 1000
            );
        });
        services.AddHostedService<XmlParseWorker>();
        services.AddHostedService<JsonPublishWorker>();
    })
    .Build();

await host.RunAsync();