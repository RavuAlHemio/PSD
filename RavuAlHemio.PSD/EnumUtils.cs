using System;

namespace RavuAlHemio.PSD
{
    internal static class EnumUtils
    {
        public static TUnderlying[] GetUnderlyingValues<TEnum, TUnderlying>()
        {
            Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
            if (underlyingType != typeof(TUnderlying))
            {
                throw new ArgumentException($"the underlying type of enum type {typeof(TEnum)} is {underlyingType}; expected {typeof(TUnderlying)}");
            }

            var values = (TEnum[]) Enum.GetValues(typeof(TEnum));
            var ret = new TUnderlying[values.Length];
            for (int i = 0; i < values.Length; ++i)
            {
                ret[i] = (TUnderlying) Convert.ChangeType(values[i], typeof(TUnderlying));
            }

            return ret;
        }
    }
}
