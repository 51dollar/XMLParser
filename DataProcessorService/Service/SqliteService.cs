using DataProcessorService.Data;
using DataProcessorService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataProcessorService.Service;

public class SqliteService(SqLiteDbContext dbContext, ILogger<SqliteService> logger)
{
    public async Task AddDateAsync(int moduleCategoryId, string moduleState, CancellationToken token)
    {
        var entity = await dbContext.Modules
            .FirstOrDefaultAsync(x => x.ModuleCategoryID == moduleCategoryId, token);

        if (entity == null)
        {
            entity = new ModuleData
            {
                ModuleCategoryID = moduleCategoryId,
                ModuleState = moduleState
            };

            await dbContext.Modules.AddAsync(entity, token);

            logger.LogInformation(
                "Создана новая запись ModuleCategoryID={ModuleCategoryID}",
                moduleCategoryId);
        }
        else
        {
            entity.ModuleState = moduleState;

            logger.LogInformation(
                "Обновлено состояние ModuleCategoryID={ModuleCategoryID}",
                moduleCategoryId);
        }

        await dbContext.SaveChangesAsync(token);
    }
}