using Models.Shared.Entities;
using Models.Shared.Models;

namespace Models.Shared.Converters;

public interface IEntityToModelConverter<TEntity, TModel> : IConverter<TEntity, TModel>
    where TEntity : class, IEntity
    where TModel : class, IModel, new()
{
}