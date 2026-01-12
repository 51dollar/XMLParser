using FileParserService.Service;
using Shared.Models.Parser.XML;

namespace FileParserService.Processing;

public class StatusChangeProcessor(ModuleStateGenerator stateGenerator)
{
    
    public bool UpdateStatus(InstrumentStatus? model, CancellationToken token)
    {
        if (model?.DeviceStatus == null)
            return false;

        bool anyUpdate = false;
            
        foreach (var status in model.DeviceStatus)
        {
            token.ThrowIfCancellationRequested();
            
            if (status.RapidControlStatus == null)
                continue;

            status.RapidControlStatus.ModuleState = stateGenerator.GetRandomState().ToString();
            anyUpdate = true;
        }
        
        return anyUpdate;
    }
}