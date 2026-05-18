using Models.Entities;
using Models.InputModels;

namespace Models.Converters;

public interface IEntityToInputConverter<TEntity, TInput>
    where TEntity : class, IEntity
    where TInput : class, IInputModel, new()
{
    static abstract TInput Convert(TEntity entity);
    static abstract TEntity Convert(TInput input);
}
