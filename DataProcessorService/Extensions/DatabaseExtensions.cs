using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DataProcessorService.Extensions;

public class DatabaseExtensions<TContext>(
    IServiceProvider serviceProvider,
    ILogger<DatabaseExtensions<TContext>> logger)
    : IHostedService
    where TContext : DbContext
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            logger.LogInformation("Применение миграций.");
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("База данных готова");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Миграции не применились");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}