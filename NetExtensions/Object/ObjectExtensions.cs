using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NetExtensions.Object {
       
       /// <summary>
       /// Helper extension methods for <see cref="object"/>
       /// </summary>
       public static class ObjectExtensions {
           /// <summary>
           /// Improvement on <see cref="object.ToString()"/> for as many .Net classes as I have to use such that outputs useful info.  
           /// </summary>
           /// <remarks>
           /// Mostly following java conventions, falls back to <c>object.ToString()</c> if it has no implementation.
           /// </remarks>
           /// <param name="obj">The object</param>
           /// <returns>The string</returns>
           public static string ToSensibleString(this object obj) {
               switch (obj) {
                   case null:
                       return null;
                   case string str:
                       return str;
                   case IDictionary dictionary:
                       return ToSensibleString(dictionary);
                   case IEnumerable enumerable:
                       return ToSensibleString(enumerable);
                   default:
                       return obj.ToString();
               }
           }

           private static string ToSensibleString(IEnumerable enumerable)
            {
                var items = new List<string>();
                foreach (var o in enumerable) {
                    items.Add(o.ToSensibleString());
                }
                
                return "[" + string.Join(", ", items) + "]";
            }

           private static string ToSensibleString(IDictionary dictionary) {
                var items = new List<string>();
                foreach (DictionaryEntry o in dictionary) {
                    items.Add(o.Key.ToSensibleString() + ": " + o.Value.ToSensibleString());
                }

                return "{" + string.Join(", ", items) + "}";
            }
       }

       public static class StringExtensions {
           public static int ToInt(this string str) {
               try {
                   return Int32.Parse(str);
               }
               catch (FormatException e) {
                   throw new FormatException("Failed to parse '" + str + "' " + e.Message);
               }
           }
           
           public static double ToDouble(this string str) {
               try {
                   return Double.Parse(str);
               }
               catch (FormatException e) {
                   throw new FormatException("Failed to parse '" + str + "' " + e.Message);
               }
           }
       }


       public static class IEqualityComparatorExtensions {
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