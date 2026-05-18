namespace Models.Entities;

public interface ILinkageEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    bool IsDeleted { get; set; }
}