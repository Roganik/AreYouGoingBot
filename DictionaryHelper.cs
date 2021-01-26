using System.Collections.Generic;

namespace AreYouGoingBot
{
    public static class DictionaryHelper
    {
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            if (!dictionary.TryGetValue(key, out var value))
            {
                value = defaultValue;
            }

            return value;
        }
    }
}