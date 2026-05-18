
namespace Models.Models;

public abstract class BaseModel : IModel
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}