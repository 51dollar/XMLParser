using DataProcessorService.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
    })
    .ConfigureServices((context, services) =>
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
            });
            
            services.AddHostedService<RabbitService>();
        })
    .Build();

await host.RunAsync();