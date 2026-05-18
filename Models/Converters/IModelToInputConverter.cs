using Models.InputModels;
using Models.Models;

namespace Models.Converters;

public interface IModelToInputConverter<TModel, TInput>
    where TModel : class, IModel
    where TInput : class, IInputModel, new()
{
    static abstract TInput Convert(TModel model);
    static abstract TModel Convert(TInput input);
}
