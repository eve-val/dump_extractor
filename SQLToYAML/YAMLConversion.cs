using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtractorLib
{
    class YAMLConversion
    {
        public static string ConvertValue(System.DBNull value)
        {
            return null;
        }
        public static string ConvertValue(string value)
        {
            return "\"" + value.Replace("\n", "\\n").Replace("\r", "") + "\"";
        }

        public static string ConvertValue(int value)
        {
            return value.ToString();
        }

        public static string ConvertValue(double value)
        {
            return "!!float " + value.ToString();
        }

        public static string ConvertValue(bool value)
        {
            return value ? "Yes" : "No";
        }

        public static string ConvertValue(object value)
        {
            //WE've failed.
            return value.ToString();
        }
    }
}
