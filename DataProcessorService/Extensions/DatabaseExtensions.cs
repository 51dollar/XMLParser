using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DataProcessorService.Extensions;

public class MigrateDatabaseService<TContext>(
    IServiceProvider serviceProvider,
    ILogger<MigrateDatabaseService<TContext>> logger)
    : IHostedService
    where TContext : DbContext
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "modules.db");
            logger.LogInformation("Проверка БД: {Path}", dbPath);

            var pendingMigrations =
                await context.Database.GetPendingMigrationsAsync(cancellationToken);

            if (pendingMigrations.Any())
            {
                await context.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Миграции успешно применены");
            }
            else
            {
                logger.LogInformation("Нет ожидающих миграций");
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Ошибка при применении миграций");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}