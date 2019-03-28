using System;
using System.Collections.Generic;

namespace NetExtensions.Collection {
    public static class EnumerableExtensions {

        /// <summary>
        /// Applies the action to every element of the <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="enumerable">The enumerable</param>
        /// <param name="action">The action</param>
        /// <typeparam name="T">The enumerable type</typeparam>
        public static void ForEach<T> (this IEnumerable<T> enumerable, Action<T> action) {
            foreach (var obj in enumerable) {
                action(obj);
            }
        }

        /// <summary>
        /// Returns the the <see cref="IEnumerable{T}"/> or a new empty <see cref="IEnumerable{T}"/> if it is <c>null</c>.
        /// </summary>
        /// <param name="enumerable">The enumerable</param>
        /// <typeparam name="T">The enumerable type</typeparam>
        /// <returns>See above.</returns>
        public static IEnumerable<T> NullToEmpty<T>(this IEnumerable<T> enumerable) {
            return enumerable ?? new T[]{};
        }

    }
}