using FileParserService.Models.Type;

namespace FileParserService.Extensions;

public static class ModuleStateExtensions
{
    private static readonly Random _random = new ();

    public static string GetRandomStateToString()
    {
        var values = Enum.GetValues<ModuleStateType>();
        var state  = values[_random.Next(values.Length)];
        return state.ToString();
    }
}