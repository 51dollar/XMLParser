using FileParserService.Extensions;
using FileParserService.Models;
using Microsoft.Extensions.Logging;

namespace FileParserService.Service;

public class StatusChangeService(ILogger logger)
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