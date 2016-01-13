using System;
using System.Linq;

namespace SystemOut.Toolbox
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input)
        {
            if (input == null)
                throw new ArgumentException("Please provide a non null string.");
            if (input == string.Empty)
                return input;
            return input.First().ToString().ToUpper() + input.Substring(1);
        }
    }
}
