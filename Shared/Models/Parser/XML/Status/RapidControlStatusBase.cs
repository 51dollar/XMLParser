using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Shared.Models.Parser.XML.Status;

[XmlInclude(typeof(CombinedSamplerStatus))]
[XmlInclude(typeof(CombinedPumpStatus))]
[XmlInclude(typeof(CombinedOvenStatus))]
[JsonDerivedType(typeof(CombinedSamplerStatus), typeDiscriminator: "CombinedSamplerStatus")]
[JsonDerivedType(typeof(CombinedPumpStatus), typeDiscriminator: "CombinedPumpStatus")]
[JsonDerivedType(typeof(CombinedOvenStatus), typeDiscriminator: "CombinedOvenStatus")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
public abstract class RapidControlStatusBase
{
    [XmlElement("ModuleState")]
    [JsonPropertyName("ModuleState")]
    public string? ModuleState { get; set; }

    [XmlElement("IsBusy")]
    [JsonPropertyName("IsBusy")]
    public bool? IsBusy { get; set; }

    [XmlElement("IsReady")]
    [JsonPropertyName("IsReady")]
    public bool? IsReady { get; set; }

    [XmlElement("IsError")]
    [JsonPropertyName("IsError")]
    public bool? IsError { get; set; }

    [XmlElement("KeyLock")]
    [JsonPropertyName("KeyLock")]
    public bool? KeyLock { get; set; }
}