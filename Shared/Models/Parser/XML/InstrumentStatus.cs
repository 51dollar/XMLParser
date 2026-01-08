using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Shared.Models.Parser.XML;

[XmlRoot("InstrumentStatus")]
public class InstrumentStatus
{
    [XmlAttribute("schemaVersion")]
    [JsonPropertyName("SchemaVersion")]
    public string SchemaVersion { get; set; } = "0.0.0";

    [XmlElement("PackageID")]
    [JsonPropertyName("PackageID")]
    public string PackageId { get; set; } = string.Empty;

    [XmlElement("DeviceStatus")]
    [JsonPropertyName("DeviceStatus")]
    public List<DeviceStatus> DeviceStatus { get; set; } =  new();
}