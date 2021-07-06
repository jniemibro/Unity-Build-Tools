using System;

namespace NBROS
{
    public static class ByteSizeExtensions
    {
        public enum SizeUnitType
        {
            Byte, KB, MB, GB, TB, PB, EB, ZB, YB
        }

        public static string ToSize(this Int64 value, SizeUnitType unit)
        {
            // Is this correct?
            return string.Format("{0}{1}s", (value / (double)Math.Pow(1024, (Int64)unit)).ToString(STRING_FORMAT), unit);
        }

        public static string ToSize(this ulong value, SizeUnitType unit)
        {
            return ((Int64)value).ToSize(unit);
        }

        const string STRING_FORMAT = "0.00";
    }
}