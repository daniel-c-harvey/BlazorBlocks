using Microsoft.AspNetCore.Components;

namespace Web.Shared.Maintenance.Entities
{
    public interface IEditModal<TModel> : IComponent
    {
        TModel Model { get; set; }
    }
}
