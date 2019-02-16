using System.Collections.Generic;

namespace NetExtensions.Collection {
    public static class SetExtensions {
        /// <summary>
        /// Adds the elements of the specified collection to the <see cref="ISet{T}"/>
        /// </summary>
        /// <param name="set">The Set</param>
        /// <param name="toAdd">The collection whose elements should be added to the <see cref="ISet{T}"/></param>
        /// <typeparam name="T">The Type of the Set</typeparam>
        public static void AddRange<T>(this ISet<T> set, IEnumerable<T> toAdd) {
            foreach (var x1 in toAdd) {
                set.Add(x1);
            }
        }
    }
}