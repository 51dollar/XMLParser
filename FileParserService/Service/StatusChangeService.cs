using FileParserService.Extensions;
using Shared.Models.Parser.XML;

namespace FileParserService.Service;

public class StatusChangeService
{
    public bool UpdateStatus(InstrumentStatus? model)
    {
        if (model?.DeviceStatus == null)
            return false;
        
        foreach (var status in model.DeviceStatus)
        {
            if (status.RapidControlStatus?.ModuleState == null && string.IsNullOrEmpty(status.RapidControlStatus?.ModuleState))
                return false;

            status.RapidControlStatus.ModuleState = ModuleStateExtensions.GetRandomStateToString();
        }
        
        return true;
    }
}