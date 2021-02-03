using System;

namespace ImageMerger_Core
{
    public static class AdditionalMath
    {
        public static T Clamp<T>(T min, T max, T value) where T : IComparable
        {
            var minValue = value.CompareTo(min) > 0 ? value : min;
            return minValue.CompareTo(max) > 0 ? max : minValue;
            ;
        }
    }
}