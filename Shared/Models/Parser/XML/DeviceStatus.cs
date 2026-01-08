using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Shared.Helper;
using Shared.Models.Parser.XML.Status;

namespace Shared.Models.Parser.XML;

public class DeviceStatus
{
    [XmlElement("ModuleCategoryID")]
    [JsonPropertyName("ModuleCategoryID")]
    public string ModuleCategoryId { get; set; } = String.Empty;

    [XmlElement("IndexWithinRole")]
    [JsonPropertyName("IndexWithinRole")]
    public int? IndexWithinRole { get; set; }

    [XmlElement("RapidControlStatus")]
    [JsonIgnore]
    public string? RapidControlStatusRaw { get; set; }

    [XmlIgnore]
    [JsonPropertyName("RapidControlStatus")]
    public RapidControlStatusBase? RapidControlStatus
    {
        get
        {
            if (_cachedStatus != null)
                return _cachedStatus;

            if (string.IsNullOrWhiteSpace(RapidControlStatusRaw))
                return null;
            
            var decoded = System.Net.WebUtility.HtmlDecode(RapidControlStatusRaw);
            
            decoded = Regex.Replace(decoded, @"<\?xml[^>]*\?>", string.Empty).Trim();
            
            var doc = new XmlDocument();
            doc.LoadXml(decoded);
            
            _cachedStatus = XmlSerializerHelper.Create(doc.DocumentElement!);
            return _cachedStatus;
        }
    }
    
    
    [XmlIgnore]
    private RapidControlStatusBase? _cachedStatus;
}