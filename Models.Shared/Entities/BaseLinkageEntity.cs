namespace Models.Shared.Entities;

public class BaseLinkageEntity : ILinkageEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}