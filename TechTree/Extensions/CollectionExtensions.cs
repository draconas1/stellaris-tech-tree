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
           
       }
   }