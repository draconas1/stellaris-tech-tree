using System;
using System.Collections.Generic;

namespace NetExtensions.Object {
    
    /// <summary>
    /// Extension and helper methods for <see cref="IEqualityComparer{T}"/>
    /// </summary>
    /// <remarks>
    /// As well as extension methods there are also factory methods designed to be accessed directly.
    /// </remarks>
    public static class IEqualityComparerExtensions {
        public static IEqualityComparer<T> Create<T>(Func<T, object> keyFunction) {
            return new FunctionEqualityComparer<T>(keyFunction);
        }

        /// <summary>
        /// Generates an IEqualityComparer where equality is tested using a simple key of the object.
        /// </summary>
        private class FunctionEqualityComparer<T> : IEqualityComparer<T> {
            private readonly Func<T, object> func;

            public FunctionEqualityComparer(Func<T, object> func) {
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