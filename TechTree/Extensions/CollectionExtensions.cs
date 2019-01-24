using System;
   using System.Collections.Generic;
   
   namespace TechTree.Extensions {
       
       /// <summary>
       /// Make c# collections useful.  Java how I miss you.  How has nobody done a standard library of these?
       /// </summary>
       public static class CollectionExtensions {
           
           public static TV ComputeIfAbsent<TK,TV>(this Dictionary<TK, TV> dictionary, TK key, Func<TK, TV> func) {
               if (!dictionary.ContainsKey(key)) {
                   dictionary[key] = func(key);
               }
   
               return dictionary[key];
           }
           
       }
   }