using Models.Entities;
using Models.Models;

namespace Models.Converters;

public interface IEntityToModelConverter<TEntity, TModel> : IConverter<TEntity, TModel>
    where TEntity : class, IEntity
    where TModel : class, IModel, new()
{
}