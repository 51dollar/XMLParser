using DataProcessorService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;

namespace DataProcessorService.Service;

public class SqliteService(SqliteDbContext dbContext, ILogger<SqliteService> logger)
{
    public async Task AddDateAsync(string? moduleCategoryId, string? moduleState, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(moduleCategoryId))
            throw new ArgumentException("ModuleCategoryId нулевой");

        if (string.IsNullOrWhiteSpace(moduleState))
            throw new ArgumentException("ModuleState нулевой");
        
        var entity = await dbContext.Modules
            .FirstOrDefaultAsync(x => x.ModuleCategoryId == moduleCategoryId, token);

        if (entity == null)
        {
            entity = new ModuleData
            {
                ModuleCategoryId = moduleCategoryId,
                ModuleState = moduleState
            };

            await dbContext.Modules.AddAsync(entity, token);
            logger.LogInformation("Создана новая запись {ModuleCategoryID} в DB", moduleCategoryId);
        }
        else
        {
            entity.ModuleState = moduleState;
            logger.LogInformation("Запись обновлена {ModuleCategoryID}", moduleCategoryId);
        }

        await dbContext.SaveChangesAsync(token);
    }
}