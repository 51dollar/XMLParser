using Shared.Models.Type;

namespace FileParserService.Service;

public class ModuleStateGenerator
{
    public ModuleStateType GetRandomState()
    {
        var values = Enum.GetValues<ModuleStateType>();
        return values[Random.Shared.Next(values.Length)];
    }
}