using Shared.Models.Parser.XML.Status;

namespace Shared.Models.Parser.JSON;

public class DeviceStatusDto
{
    public string? ModuleCategoryID { get; set; }
    public int? IndexWithinRole { get; set; }
    public RapidControlStatusBase? RapidControlStatus { get; set; }
}