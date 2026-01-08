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
            services.AddDbContext<SqliteDbContext>(options =>
                options.UseSqlite(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")));
            services.AddHostedService<DatabaseExtensions<SqliteDbContext>>();
            services.AddScoped<SqliteService>();
            services.AddHostedService<RabbitService>();
        })
    .Build();

await host.RunAsync();