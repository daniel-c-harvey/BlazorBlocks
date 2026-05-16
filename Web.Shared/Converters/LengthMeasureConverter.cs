using MudBlazor;

namespace Web.Shared.Converters
{
    public static class LengthMeasureConverter
    {
        public static Converter<decimal> GetConverter { get; } = new()
        {
            GetFunc = text => NetBlocks.Utilities.LengthMeasureConverter.ToDecimalFeet(text ?? "0"),
            SetFunc = value => NetBlocks.Utilities.LengthMeasureConverter.ToImperialString(value)
        };
    }
}
