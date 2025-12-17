using DataProcessorService.Data;
using DataProcessorService.Extensions;
using DataProcessorService.Service;
using Microsoft.EntityFrameworkCore;
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
            
            var dbPath = Path.Combine(AppContext.BaseDirectory, "modules.db");

            services.AddDbContext<SqLiteDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));
            services.AddHostedService<MigrateDatabaseService<SqLiteDbContext>>();
            services.AddScoped<SqliteService>();
            services.AddHostedService<RabbitService>();
        })
    .Build();

await host.RunAsync();