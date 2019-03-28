using System;

namespace NetExtensions.Object {
    public static class StringExtensions {
        public static int ToInt(this string str) {
            try {
                return int.Parse(str);
            }
            catch (FormatException e) {
                throw new FormatException("Failed to parse '" + str + "' " + e.Message);
            }
        }
           
        public static double ToDouble(this string str) {
            try {
                return double.Parse(str);
            }
            catch (FormatException e) {
                throw new FormatException("Failed to parse '" + str + "' " + e.Message);
            }
        }
    }
}