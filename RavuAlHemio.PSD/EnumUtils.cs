using System;

namespace RavuAlHemio.PSD
{
    /// <summary>
    /// Enumeration utilities.
    /// </summary>
    internal static class EnumUtils
    {
        /// <summary>
        /// Obtains the values of the given enumeration converted to the integer type underlying the enumeration.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type whose values to return.</typeparam>
        /// <typeparam name="TUnderlying">
        /// The integer type underlying the <typeparamref name="TEnum"/> enumeration type.
        /// </typeparam>
        /// <returns>The values of the given enumeration converted to the type underlying it.</returns>
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
