using FileParserService.Extensions;
using Shared.Models;

namespace FileParserService.Service;

public class StatusChangeService
{
    public bool UpdateStatus(ModelXmlParse? model)
    {
        if (model?.DeviceStatus == null)
            return false;
        
        foreach (var deviceState in model.DeviceStatus)
        {
            foreach (var rapidControlState in deviceState.RapidControlStatus)
            {
                foreach (var combinedSamplerState in rapidControlState.CombinedSamplerStatus)
                {
                    combinedSamplerState.ModuleState = ModuleStateExtensions.GetRandomStateToString();
                }
            }
        }
        
        return true;
    }
}