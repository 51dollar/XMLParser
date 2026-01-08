using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Shared.Models.Parser.XML.Status;

[XmlRoot("CombinedSamplerStatus")]
public class CombinedSamplerStatus : RapidControlStatusBase
{
    [XmlElement("Status")]
    [JsonPropertyName("Status")]
    public int? Status { get; set; }

    [XmlElement("Vial")]
    [JsonPropertyName("Vial")]
    public string? Vial { get; set; }

    [XmlElement("Volume")]
    [JsonPropertyName("Volume")]
    public decimal? Volume { get; set; }

    [XmlElement("MaximumInjectionVolume")]
    [JsonPropertyName("MaximumInjectionVolume")]
    public decimal? MaximumInjectionVolume { get; set; }

    [XmlElement("RackL")]
    [JsonPropertyName("RackL")]
    public string? RackL { get; set; }

    [XmlElement("RackR")]
    [JsonPropertyName("RackR")]
    public string? RackR { get; set; }

    [XmlElement("RackInf")]
    [JsonPropertyName("RackInf")]
    public string? RackInf { get; set; }

    [XmlElement("Buzzer")]
    [JsonPropertyName("Buzzer")]
    public bool? Buzzer { get; set; }
}