using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace CodeFactoryExtensions.Formatting.CSharp
{
    /// <summary>
    /// Extensions class that will format data into different programing case types.
    /// </summary>
    public static class CsCaseManagementExtensions
    {
        /// <summary>
        /// Converts a target string to camel case.
        /// </summary>
        /// <param name="source">The source string to convert to camel case.</param>
        /// <param name="trimPrefixCharacters">Optional parameter for characters to trim from the start of the string if they exist.</param>
        /// <returns>The formatted string or null if the string cant be formatted.</returns>
        public static string ConvertToCamelCase(this string source, char[] trimPrefixCharacters = null)
        {
            if (string.IsNullOrEmpty(source)) return null;

            string result = source;

            if (trimPrefixCharacters != null)
            {
                bool changed = true;
                while (changed)
                {
                    var trimResult = result.TrimFirstCharacter(trimPrefixCharacters);
                    changed = trimResult.changed;
                    result = trimResult.updatedString;
                }
            }

            if (string.IsNullOrEmpty(result)) return result;

            if (char.IsUpper(result[0]))
            {
                result = result.Length > 1 ? $"{char.ToLower(result[0])}{result.Substring(1)}" : $"{char.ToLower(result[0])}";
            }

            return result;

        }

        /// <summary>
        /// Converts a target string to camel case.
        /// </summary>
        /// <param name="source">The source string to convert to camel case.</param>
        /// <param name="trimPrefixCharacters">Optional parameter for characters to trim from the start of the string if they exist.</param>
        /// <returns>The formatted string or null if the string cant be formatted.</returns>
        public static string ConvertToProperCase(this string source, char[] trimPrefixCharacters = null)
        {
            if (string.IsNullOrEmpty(source)) return null;

            string result = source;

            if (trimPrefixCharacters != null)
            {
                bool changed = true;
                while (changed)
                {
                    var trimResult = result.TrimFirstCharacter(trimPrefixCharacters);
                    changed = trimResult.changed;
                    result = trimResult.updatedString;
                }
            }

            if (string.IsNullOrEmpty(result)) return result;


            if (char.IsLower(result[0]))
            {
                result = result.Length > 1
                    ? $"{char.ToUpper(result[0])}{result.Substring(1)}" : $"{char.ToUpper(result[0])}";
            }

            return result;

        }

        /// <summary>
        /// Extension method that will trim the white space and the leading character if it is one of the target trim characters.
        /// </summary>
        /// <param name="source">String to trim.</param>
        /// <param name="trimCharacters">The target characters to trim.</param>
        /// <returns>Tuple with the trim status and the updated string.</returns>
        public static (bool changed, string updatedString) TrimFirstCharacter(this string source, char[] trimCharacters)
        {
            bool changed = false;

            string result = source;

            if (string.IsNullOrEmpty(result)) return (changed, result);
            if(trimCharacters == null) return (changed, result);

            result = source.Trim();

            char check = result[0];

            changed = trimCharacters.Any(c => c == check);

            return changed ? (changed, result.Substring(1)) : (changed, result);
        }


    }
}
