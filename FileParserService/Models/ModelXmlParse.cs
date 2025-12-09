using System.Xml.Serialization;
using FileParserService.Models.InstrumentStatus;

namespace FileParserService.Models;

[XmlRoot("InstrumentStatus")]
public class ModelXmlParse
{
    [XmlElement("PackageID")]
    public string PackageID { get; set; }
    
    [XmlElement("DeviceStatus")]
    public DeviceStatus[] DeviceStatus { get; set; }
}