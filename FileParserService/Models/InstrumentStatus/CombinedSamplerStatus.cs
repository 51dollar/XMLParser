using System.Xml.Serialization;

namespace FileParserService.Models.InstrumentStatus;

[XmlRoot("CombinedSamplerStatus")]
public class CombinedSamplerStatus
{
    [XmlElement("ModuleState")]
    public string ModuleState { get; set; }
    [XmlElement("IsBusy")]
    public bool IsBusy { get; set; }
    [XmlElement("IsReady")]
    public bool IsReady { get; set; }
    [XmlElement("IsError")]
    public bool IsError { get; set; }
    [XmlElement("KeyLock")]
    public bool KeyLock { get; set; }
    [XmlElement("Status")]
    public int Status { get; set; }
    [XmlElement("Vial")]
    public string Vial { get; set; }
    [XmlElement("Volume")]
    public decimal Volume { get; set; }
    [XmlElement("MaximumInjectionVolume")]
    public decimal MaximumInjectionVolume { get; set; }
    [XmlElement("RackL")]
    public string RackL { get; set; }
    [XmlElement("RackR")]
    public string RackR { get; set; }
    [XmlElement("RackInf")]
    public int RackInf { get; set; }
    [XmlElement("Buzzer")]
    public bool Buzzer { get; set; }
}