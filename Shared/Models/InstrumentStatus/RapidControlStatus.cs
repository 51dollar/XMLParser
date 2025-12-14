using System.Xml.Serialization;

namespace Shared.Models.InstrumentStatus;

public class RapidControlStatus
{
    [XmlElement("CombinedSamplerStatus")]
    public CombinedSamplerStatus[]  CombinedSamplerStatus { get; set; } 
}