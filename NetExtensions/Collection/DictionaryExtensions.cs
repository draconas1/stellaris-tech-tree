using System;
using System.Collections.Generic;

namespace NetExtensions.Collection {
    public static class DictionaryExtensions {
        /// <summary>
        /// <para>If the specified key is not already associated with a value (or is mapped to <c>null</c>), attempts to compute
        /// its value using the given mapping function and enters it into this dictionary unless <c>null</c>.</para>
        /// <para>If the function returns null no mapping is recorded. If the function itself throws an exception,
        /// the exception is rethrown, and no mapping is recorded. The most common usage is to construct a new object
        /// serving as an initial mapped value or memoized result. </para>
        /// </summary>
        /// <example>
        /// <code>
        ///    dictionary.ComputeIfAbsent(key, k => new Value());
        /// </code>
        /// </example>
        /// <example>
        ///  <code>
        ///    dictionary.ComputeIfAbsent(key, k => new List()).Add("value");
        /// </code>
        /// </example>
        /// <param name="dictionary">The Dictionary</param>
        /// <param name="key">The key with which the specified value is to be associated</param>
        /// <param name="func">The function to compute a value</param>
        /// <typeparam name="TK">The key type</typeparam>
        /// <typeparam name="TV">The value Type</typeparam>
        /// <returns>the current (existing or computed) value associated with the specified key, or null if the computed value is null</returns>
        public static TV ComputeIfAbsent<TK, TV>(this IDictionary<TK, TV> dictionary, TK key, Func<TK, TV> func) {
            if (!dictionary.ContainsKey(key)) {
                dictionary[key] = func(key);
            }

            return dictionary[key];
        }

        /// <summary>
        /// Puts the contents of an <see cref="IEnumerable{T}"/> into the dictionary.  Existing keys will be overriden.
        /// </summary>
        /// <param name="dictionary">The Dictionary</param>
        /// <param name="toPut">The <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/>s to add</param>
        /// <typeparam name="TK">The key type</typeparam>
        /// <typeparam name="TV">The value Type</typeparam>
        public static void PutAll<TK, TV>(this IDictionary<TK, TV> dictionary,
            IEnumerable<KeyValuePair<TK, TV>> toPut) {
            foreach (var keyValuePair in toPut) {
                dictionary[keyValuePair.Key] = keyValuePair.Value;
            }
        }
    }
}