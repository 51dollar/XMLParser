using System.Xml;
using System.Xml.Serialization;
using Shared.Models.Parser.XML.Status;

namespace Shared.Helper;

public static class XmlSerializerHelper
{
    private static readonly Dictionary<string, Type> Map = new()
    {
        ["CombinedSamplerStatus"] = typeof(CombinedSamplerStatus),
        ["CombinedPumpStatus"]    = typeof(CombinedPumpStatus),
        ["CombinedOvenStatus"]    = typeof(CombinedOvenStatus),
    };

    public static RapidControlStatusBase? Create(XmlNode node)
    {
        if (node == null)
            return null;

        if (!Map.TryGetValue(node.LocalName, out var type))
            return null;

        var serializer = new XmlSerializer(type);
        using var reader = new StringReader(node.OuterXml);
        return (RapidControlStatusBase?)serializer.Deserialize(reader);
    }
}