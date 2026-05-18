using Models.Shared.InputModels;
using Models.Shared.Models;

namespace Models.Shared.Converters;

public interface IModelToInputConverter<TModel, TInput>
    where TModel : class, IModel
    where TInput : class, IInputModel, new()
{
    static abstract TInput Convert(TModel model);
    static abstract TModel Convert(TInput input);
}
