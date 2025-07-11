using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace iEngr.Hookup.Models
{
    public static class GeneralFun
    {
        public static object ParseNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;
            // 尝试解析为int
            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
                return intValue;
            // 尝试解析为double
            if (double.TryParse(input, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out double doubleValue))
                return doubleValue;
            return null;
        }

            public static string ConvertToSqlInString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "('')";

            // 过滤特殊字符（防止SQL注入）
            var safeItems = input.Split(',')
                                 .Select(item => item.Trim())
                                 .Where(item => !string.IsNullOrWhiteSpace(item))
                                 .Select(item => item.Replace("'", "''")) // 处理单引号
                                 .ToList();

            if (!safeItems.Any())
                return "('')";

            return $"('{string.Join("','", safeItems)}')";
        }
        public static string ConvertToStringScope(string input, char delimiter)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "('')";

            // 分割、清理、去重
            var distinctItems = input.Split(delimiter)
                                     .Select(item => item.Trim())
                                     .Where(item => !string.IsNullOrWhiteSpace(item))
                                     .Distinct(StringComparer.OrdinalIgnoreCase) // 不区分大小写去重
                                     .ToList();

            if (!distinctItems.Any())
                return "('')";

            return $"('{string.Join("','", distinctItems)}')";
        }
        public static string ConvertToNumScope(string input, char delimiter)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "('')";

            // 分割、清理、去重
            var distinctItems = input.Split(delimiter)
                                     .Select(item => item.Trim())
                                     .Where(item => !string.IsNullOrWhiteSpace(item))
                                     .Distinct(StringComparer.OrdinalIgnoreCase) // 不区分大小写去重
                                     .ToList();

            if (!distinctItems.Any())
                return "('')";

            return $"({string.Join(",", distinctItems)})";
        }
        public static string ConvertToSqlString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var expItems = input.Split(':')
                                 .Select(item => item.Trim())
                                 .ToList();

            if (expItems.Count != 3) return null;
            {
                switch (expItems[1].ToLower())
                {
                    case "in":
                        return expItems[0] + " in  " + ConvertToStringScope(expItems[2], '|');
                    case "out":
                        return expItems[0] + " not in  " + ConvertToStringScope(expItems[2], '|');
                    case "=":
                        return expItems[0] + " in  " + ConvertToNumScope(expItems[2], '|');
                    case "!=":
                        return expItems[0] + " not in  " + ConvertToNumScope(expItems[2], '|');
                    case "<":
                    case "<=":
                    case ">":
                    case ">=":
                        if (ParseNumber(expItems[2]) == null)
                            return null;
                        else
                            return expItems[0] + " " + expItems[1] + " " + ParseNumber(expItems[2]);
                    case "<>":
                    case "<=>":
                    case "<>=":
                    case "<=>=":
                        var varMin = ParseNumber(expItems[2].Split('~')
                                     .Select(item => item.Trim())
                                     .Where(item => !string.IsNullOrWhiteSpace(item)).First());
                        var varMax = ParseNumber(expItems[2].Split('~')
                                     .Select(item => item.Trim())
                                     .Where(item => !string.IsNullOrWhiteSpace(item)).Last());
                        if (varMin != null && varMax != null)
                        {
                            string strL = expItems[1].Contains("<=") ? "<=" : "<";
                            string strH = expItems[1].Contains(">=") ? ">=" : ">";

                            return expItems[0] + " " + strL + " " + varMax + " and " + expItems[0] + " " + strH + " " + varMin;

                        }
                        else
                            return null;
                    default:
                        return null;

                }
            }
        }

    }
}
