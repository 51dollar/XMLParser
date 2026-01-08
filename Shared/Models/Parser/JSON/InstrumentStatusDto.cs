namespace Shared.Models.Parser.JSON;

public class InstrumentStatusDto
{
    public string? SchemaVersion { get; set; }
    public string? PackageID { get; set; }
    public List<DeviceStatusDto>? DeviceStatus { get; set; } = new();
}
