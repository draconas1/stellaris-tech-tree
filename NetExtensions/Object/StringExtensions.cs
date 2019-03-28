using System;

namespace NetExtensions.Object {
    
    /// <summary>
    /// Extension methods for <see cref="string"/>
    /// </summary>
    public static class StringExtensions {
        
        /// <summary>
        /// Converts the value to <see cref="int"/> using <see cref="int#parse"/> and improves the exception handling.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The integer value of the string</returns>
        /// <exception cref="FormatException">A format exception that will include the string value if <see cref="int#parse"/> throws a <see cref="FormatException"/></exception>
        public static int ToInt(this string str) {
            try {
                return int.Parse(str);
            }
            catch (FormatException e) {
                throw new FormatException("Failed to parse '" + str + "' " + e.Message);
            }
        }
         
        /// <summary>
        /// Converts the value to <see cref="double"/> using <see cref="double#parse"/> and improves the exception handling.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The double value of the string</returns>
        /// <exception cref="FormatException">A format exception that will include the string value if <see cref="double#parse"/> throws a <see cref="FormatException"/></exception>
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