using System;
using System.Collections.Generic;
using System.Linq;

namespace Provausio.Core.Ext
{
    public static class StringExt
    {
        /// <summary>
        /// Determines whether this instance is numeric.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static bool IsNumeric(this string target)
        {
            return target.Cast<char>().All(char.IsDigit);
        }

        /// <summary>
        /// Attempts to parse the enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        public static T FindEnum<T>(this string target, bool ignoreCase = true)
            where T : struct
        {
            return (T) Enum.Parse(typeof(T), target, ignoreCase);
        }

        /// <summary>
        /// Attempts to parse the enum. Returns true or false, indicating success or failure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The string that will be parsed in to the specified type.</param>
        /// <param name="result">If successful, will contain the parsed result. If not successful, this will be equal to the default of T</param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static bool TryFindEnum<T>(this string target, out T result, bool ignoreCase = true)
            where T : struct
        {
            try
            {
                result = FindEnum<T>(target, ignoreCase);
                return true;
            }
            catch (Exception)
            {
                result = default(T);
                return false;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="formatter">The formatter.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public static string ToString(this object obj, IObjectStringFormatter formatter)
        {
            return formatter.ToString(obj);
        }

        /// <summary>
        /// Returns whether or not the target value is null, empty, or pure whitespace
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsNullOrEmptyOrWhitespace(this string target)
        {
            return string.IsNullOrEmpty(target) || string.IsNullOrWhiteSpace(target);
        }

        /// <summary>
        /// Reformats the input string to capitalize the first letter of every word.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToTitleCase(this string input)
        {
            var fixedWords = new List<string>();
            foreach (var word in input.Split(' '))
            {
                var modified = word.ToLower().ToCharArray();
                modified[0] = char.ToUpper(modified[0]);
                fixedWords.Add(new string(modified));
            }

            return string.Join(" ", fixedWords);
        }

        /// <summary>
        ///     Ensures that the input length is no longer than the specified maxLength. If it is longer, then 
        ///     it will be truncated to the maximum length. If ellipsis is enabled, then it will be added to the 
        ///     max length string. For example, if max length is 3 and the input is truncated, then the total result 
        ///     length will be 3 + 3 (ellipsis) or 6 in total.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="maxLength"></param>
        /// <param name="addEllipsis"></param>
        /// <returns></returns>
        public static string Truncate(this string input, int maxLength, bool addEllipsis = true)
        {
            if (input.Length <= maxLength)
                return input;

            var truncated = input.Substring(0, maxLength);
            if (addEllipsis)
                truncated += "...";

            return truncated;
        }
    }

    public interface IObjectStringFormatter
    {
        string ToString(object input);
    }

    public class DefaultObjectStringFormatter : IObjectStringFormatter
    {
        public string ToString(object input)
        {
            return input.ToString();
        }
    }
}
