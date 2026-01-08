using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Shared.Models.Parser.XML.Status;

[XmlRoot("CombinedPumpStatus")]
public class CombinedPumpStatus : RapidControlStatusBase
{
    [XmlElement("Mode")]
    [JsonPropertyName("Mode")]
    public string? Mode { get; set; }

    [XmlElement("Flow")]
    [JsonPropertyName("Flow")]
    public decimal? Flow { get; set; }

    [XmlElement("PercentB")]
    [JsonPropertyName("PercentB")]
    public decimal? PercentB { get; set; }

    [XmlElement("PercentC")]
    [JsonPropertyName("PercentC")]
    public decimal? PercentC { get; set; }

    [XmlElement("PercentD")]
    [JsonPropertyName("PercentD")]
    public decimal? PercentD { get; set; }

    [XmlElement("MinimumPressureLimit")]
    [JsonPropertyName("MinimumPressureLimit")]
    public decimal? MinimumPressureLimit { get; set; }

    [XmlElement("MaximumPressureLimit")]
    [JsonPropertyName("MaximumPressureLimit")]
    public decimal? MaximumPressureLimit { get; set; }

    [XmlElement("Pressure")]
    [JsonPropertyName("Pressure")]
    public decimal? Pressure { get; set; }

    [XmlElement("PumpOn")]
    [JsonPropertyName("PumpOn")]
    public bool? PumpOn { get; set; }

    [XmlElement("Channel")]
    [JsonPropertyName("Channel")]
    public int? Channel { get; set; }
}