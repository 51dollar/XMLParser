using DataProcessorService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;

namespace DataProcessorService.Service;

public class SqliteService(SqliteDbContext dbContext, ILogger<SqliteService> logger)
{
    public async Task AddDateAsync(string? moduleCategoryId, string? moduleState, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(moduleCategoryId)
            || string.IsNullOrWhiteSpace(moduleState))
        {
            logger.LogWarning(
                "Пропуск записи: некорректные данные. CategoryId={CategoryId}, State={State}",
                moduleCategoryId,
                moduleState);
            return;
        }

        try
        {
            var entity = await dbContext.Modules
                .FirstOrDefaultAsync(x => x.ModuleCategoryId == moduleCategoryId, token);

            if (entity == null)
            {
                entity = new ModuleData
                {
                    ModuleCategoryId = moduleCategoryId,
                    ModuleState = moduleState
                };

                dbContext.Modules.Add(entity);
                logger.LogInformation("Создана новая запись {ModuleCategoryID} в DB", moduleCategoryId);
            }
            else
            {
                entity.ModuleState = moduleState;
                logger.LogDebug("Запись обновлена {ModuleCategoryID}", moduleCategoryId);
            }

            await dbContext.SaveChangesAsync(token);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Конфликт при сохранении ModuleCategoryId={CategoryId}",
                moduleCategoryId);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Сохранение отменено. ModuleCategoryId={CategoryId}",
                moduleCategoryId);
        }
    }
}