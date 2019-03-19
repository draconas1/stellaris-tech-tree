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

       /// <summary>
       /// Generates an IEqualityComparer where equality is tested using a simple key of the object.
       /// </summary>
       public static class IEqualityComparatorExtensions {
           public static IEqualityComparer<T> Create<T>(Func<T, object> keyFunction) {
               return new Comparer<T>(keyFunction);
           }

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