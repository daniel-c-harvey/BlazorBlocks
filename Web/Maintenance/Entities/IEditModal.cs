using Microsoft.AspNetCore.Components;

namespace Web.Maintenance.Entities
{
    public interface IEditModal<TModel> : IComponent
    {
        TModel Model { get; set; }
    }
}
