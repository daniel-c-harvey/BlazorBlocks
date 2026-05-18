namespace Models.Shared.Converters;

public interface IConverter<T2, T1>
{
    static abstract T1 Convert(T2 entity);
    static abstract T2 Convert(T1 model);
}