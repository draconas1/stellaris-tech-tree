using System;
using System.Collections.Generic;

namespace NetExtensions.Object {
    public static class IComparerExtensions {
        /// <summary>
        /// Generates an IComparer where the objects will be compared by extracting a single property using the first supplied function, if that returns equal (0) then the next property will be extracted and compared etc.    
        /// </summary>
        /// <remarks>
        /// Based off javas Comparator.comparing(Function).  Why does this not exist in c#?
        /// </remarks>
        public static IComparer<T> Create<T>(params Func<T, IComparable>[] keyFunctions) {
            IComparer<T> comparer = new Comparer<T>(keyFunctions[0]);
            if (keyFunctions.Length == 1) {
                return comparer;
            }

            for (int i = 1; i < keyFunctions.Length; i++) {
                comparer = comparer.ThenComparing(keyFunctions[i]);
            }

            return comparer;
        }

        /// <summary>
        /// Chains 2 comparators, such that the 2nd will be used if the first returns 0.
        /// </summary>
        /// <param name="comparator">The first comparator</param>
        /// <param name="thenComparing">A function to extract the field that will then be compared upon</param>
        ///  /// <remarks>
        /// Based off javas Comparator.thenComparing(Function).  Why does this not exist in c#?
        /// </remarks>
        public static IComparer<T> ThenComparing<T>(this IComparer<T> comparator, Func<T, IComparable> thenComparing) {
            return new ChainedComparer<T>(comparator, new Comparer<T>(thenComparing));
        }

        private class ChainedComparer<T> : IComparer<T> {
            private readonly IComparer<T> comp1;
            private readonly IComparer<T> comp2;

            internal ChainedComparer(IComparer<T> comp1, IComparer<T> comp2) {
                this.comp1 = comp1;
                this.comp2 = comp2;
            }
            public int Compare(T x, T y) {
                var compare = comp1.Compare(x, y);
                return compare == 0 ? comp2.Compare(x, y) : compare;
            }
        } 

        private class Comparer<T> : IComparer<T> {
            private readonly Func<T, IComparable> func;

            internal Comparer(Func<T, IComparable> func) {
                this.func = func;
            }
            public int Compare(T x, T y) {
                return func(x).CompareTo(func(y));
            }
        }
    }
}