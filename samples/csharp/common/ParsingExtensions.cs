using System.Globalization;

namespace Common
{
    public static class ParsingExtensions
    {
        public static float ToFloatWithInvariantCulture(this string stringToCast) => float.Parse(stringToCast, CultureInfo.InvariantCulture);
    }
}
