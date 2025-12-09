using System.Xml.Serialization;

namespace FileParserService.Models.InstrumentStatus;

public class RapidControlStatus
{
    [XmlElement("CombinedSamplerStatus")]
    public CombinedSamplerStatus[]  CombinedSamplerStatus { get; set; } 
}