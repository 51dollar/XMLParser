using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Models.Parser.XML;

namespace FileParserService.Parsing;

public class JsonMessageSerializer
{
    public byte[] ConvertToJson(InstrumentStatus? model, bool indented = true)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        string json = System.Text.Json.JsonSerializer.Serialize(model, options);
        return Encoding.UTF8.GetBytes(json);
    }
}