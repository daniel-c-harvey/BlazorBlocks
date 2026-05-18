
namespace Models.Shared.Models;

public interface IModel : IKeyed
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}