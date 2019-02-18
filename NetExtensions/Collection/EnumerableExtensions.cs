using System;
using System.Collections.Generic;

namespace NetExtensions.Collection {
    public static class EnumerableExtensions {

        /// <summary>
        /// Applies the action to every element of the <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="enumerable">The enumerable</param>
        /// <param name="action">The action</param>
        /// <typeparam name="T">The action type</typeparam>
        /// <typeparam name="TU">The enumerable type, which must extend <c>T</c></typeparam>
        public static void ForEach<T> (this IEnumerable<T> enumerable, Action<T> action) {
            foreach (var x1 in enumerable) {
                action(x1);
            }
        }

    }
}