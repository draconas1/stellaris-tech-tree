using System;
   using System.Collections.Generic;
using System.Linq;

namespace TechTree.Extensions {
       
       /// <summary>
       /// Make c# collections useful.  Java how I miss you.  How has nobody done a standard library of these?
       /// </summary>
       public static class CollectionExtensions {

            public static string ToSensibleString<TV>(this IEnumerable<TV> enumerable)
            {
                return "[" + string.Join(", ", enumerable) + "]";
            }

            public static string ToSensibleString<TK, TV>(this IDictionary<TK, TV> dictionary)
            {
                return "{" + string.Join(", ", dictionary.Select(entry => entry.Key + ": " + entry.Value)) + "}";
            }

           public static TV ComputeIfAbsent<TK,TV>(this IDictionary<TK, TV> dictionary, TK key, Func<TK, TV> func) {
               if (!dictionary.ContainsKey(key)) {
                   dictionary[key] = func(key);
               }
   
               return dictionary[key];
           }

           /// <summary>
           /// AddRange is on a list but not on a MFing set!
           /// </summary>
           /// <param name="set"></param>
           /// <param name="toAdd"></param>
           /// <typeparam name="T"></typeparam>
           public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> toAdd) {
               foreach (var x1 in toAdd) {
                   set.Add(x1);
               }
           }
           
       }
   }