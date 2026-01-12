namespace Shared.Entity;

public class ModuleData
{
    public Guid Id { get; set; }
    public required string ModuleCategoryId { get; set; }
    public required string ModuleState { get; set; }
}