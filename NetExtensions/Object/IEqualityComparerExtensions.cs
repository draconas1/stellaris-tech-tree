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
        public static IEqualityComparer<T> Create<T>(params Func<T, object>[] keyFunction) {
            return new FunctionEqualityComparer<T>(keyFunction);
        }

        /// <summary>
        /// Generates an IEqualityComparer where equality is tested using a simple key of the object.
        /// </summary>
        private class FunctionEqualityComparer<T> : IEqualityComparer<T> {
            private readonly Func<T, object>[] funcs;

            public FunctionEqualityComparer(Func<T, object>[] funcs) {
                this.funcs = funcs;
            }
               
            public bool Equals(T x, T y) {
                
                foreach (var func in funcs) {
                    bool @equals = Equals(func(x), func(y));
                    if (!equals) {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(T obj) {
                int hash = 1;
                foreach (var func in funcs) {
                    hash = hash * func(obj).GetHashCode();
                }

                return hash;
            }
        }
    }
}