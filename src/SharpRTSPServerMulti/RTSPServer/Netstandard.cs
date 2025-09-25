using System;
using System.Collections.Generic;
using System.Text;

#if NETSTANDARD2_0
namespace System
{
    internal static class NullableExtensions
    {
        public static T GetValueOrDefault<T>(this T? value, T defaultValue = default) where T : struct
        {
            return value.HasValue ? value.Value : defaultValue;
        }
    }

    namespace System.Collections.Generic
    {
        internal static class DictionaryExtensions
        {
            public static TValue? GetValueOrDefault<TKey, TValue>(
                this IDictionary<TKey, TValue> dictionary,
                TKey key,
                TValue? defaultValue = default)
            {
                if(dictionary == null) throw new ArgumentNullException(nameof(dictionary));
                return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
            }
        }
    }
}
#endif