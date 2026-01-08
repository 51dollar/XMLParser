using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Shared.Models.Parser.XML.Status;

[XmlRoot("CombinedOvenStatus")]
public class CombinedOvenStatus : RapidControlStatusBase
{
    [XmlElement("UseTemperatureControl")]
    [JsonPropertyName("UseTemperatureControl")]
    public bool? UseTemperatureControl { get; set; }

    [XmlElement("OvenOn")]
    [JsonPropertyName("OvenOn")]
    public bool? OvenOn { get; set; }

    [XmlElement("Temperature_Actual")]
    [JsonPropertyName("TemperatureActual")]
    public decimal? TemperatureActual { get; set; }

    [XmlElement("Temperature_Room")]
    [JsonPropertyName("TemperatureRoom")]
    public decimal? TemperatureRoom { get; set; }

    [XmlElement("MaximumTemperatureLimit")]
    [JsonPropertyName("MaximumTemperatureLimit")]
    public decimal? MaximumTemperatureLimit { get; set; }

    [XmlElement("Valve_Position")]
    [JsonPropertyName("ValvePosition")]
    public int? ValvePosition { get; set; }

    [XmlElement("Valve_Rotations")]
    [JsonPropertyName("ValveRotations")]
    public int? ValveRotations { get; set; }

    [XmlElement("Buzzer")]
    [JsonPropertyName("Buzzer")]
    public bool? Buzzer { get; set; }
}