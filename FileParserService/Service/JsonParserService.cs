using System.Text;
using System.Text.Json;
using Shared.Models;

namespace FileParserService.Service;

public class JsonParserService
{
    public byte[] ConvertToJson(ModelXmlParse? model, bool indented = true)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented
        };

        string json = JsonSerializer.Serialize(model, options);
        return Encoding.UTF8.GetBytes(json);
    }
}