using System;
using System.Collections.Generic;

namespace NetExtensions.Object {
    public static class IEqualityComparerExtensions {
        public static IEqualityComparer<T> Create<T>(Func<T, object> keyFunction) {
            return new Comparer<T>(keyFunction);
        }

        /// <summary>
        /// Generates an IEqualityComparer where equality is tested using a simple key of the object.
        /// </summary>
        private class Comparer<T> : IEqualityComparer<T> {
            private readonly Func<T, object> func;

            public Comparer(Func<T, object> func) {
                this.func = func;
            }
               
            public bool Equals(T x, T y) {
                return Equals(func(x), func(y));
            }

            public int GetHashCode(T obj) {
                return func(obj).GetHashCode();
            }
        }
    }
}