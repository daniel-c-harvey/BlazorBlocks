
namespace Models.Shared.Entities;

public interface IEntity : IKeyed
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    bool IsDeleted { get; set; }
}