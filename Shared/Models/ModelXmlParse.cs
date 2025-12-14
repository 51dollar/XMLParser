using System.Xml.Serialization;
using Shared.Models.InstrumentStatus;

namespace Shared.Models;

[XmlRoot("InstrumentStatus")]
public class ModelXmlParse
{
    [XmlElement("PackageID")]
    public string PackageID { get; set; }
    
    [XmlElement("DeviceStatus")]
    public DeviceStatus[] DeviceStatus { get; set; }
}