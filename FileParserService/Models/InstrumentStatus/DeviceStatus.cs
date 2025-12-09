using System.Xml.Serialization;

namespace FileParserService.Models.InstrumentStatus;

public class DeviceStatus
{
    [XmlElement("ModuleCategoryID")]
    public string ModuleCategoryID { get; set; }

    [XmlElement("IndexWithinRole")]
    public int IndexWithinRole { get; set; }
    
    [XmlElement("RapidControlStatus")]
    public RapidControlStatus[] RapidControlStatus { get; set; }
}