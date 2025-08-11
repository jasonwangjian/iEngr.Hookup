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
 
        /// <summary>
        /// 解析数字值 (支持整型和浮点型)
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>数字对象 (int/double) 或 null</returns>
        public static object ParseNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // 尝试解析为整数
            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
                return intValue;

            // 尝试解析为双精度浮点数
            if (double.TryParse(input,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out double doubleValue))
            {
                return doubleValue;
            }

            return null;
        }

        /// <summary>
        /// 转换字符串集合为SQL IN格式
        /// </summary>
        /// <param name="input">输入字符串 (用分隔符分隔)</param>
        /// <param name="delimiter">分隔符</param>
        /// <returns>SQL格式的字符串集合 如 ('a','b')</returns>
        public static string ConvertToStringScope(string input, char delimiter)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "('')";

            // 处理字符串集合：分割、清理、去重
            var distinctItems = input.Split(delimiter)
                                     .Select(item => item.Trim())
                                     .Where(item => !string.IsNullOrWhiteSpace(item))
                                     .Distinct(StringComparer.OrdinalIgnoreCase)
                                     .ToList();

            return distinctItems.Count > 0
                ? $"('{string.Join("','", distinctItems)}')"
                : "('')";
        }

        /// <summary>
        /// 转换数字集合为SQL IN格式
        /// </summary>
        /// <param name="input">输入字符串 (用分隔符分隔)</param>
        /// <param name="delimiter">分隔符</param>
        /// <returns>SQL格式的数字集合 如 (1,2,3)</returns>
        public static string ConvertToNumScope(string input, char delimiter)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "(NULL)";

            // 处理数字集合：分割、清理、解析、去重
            var validNumbers = input.Split(delimiter)
                                    .Select(item => item.Trim())
                                    .Where(item => !string.IsNullOrWhiteSpace(item))
                                    .Select(ParseNumber)
                                    .Where(num => num != null)
                                    .Distinct()
                                    .ToList();

            return validNumbers.Count > 0
                ? $"({string.Join(",", validNumbers)})"
                : "(NULL)";
        }
        /// <summary>
        /// 解析条件表达式为SQL条件语句
        /// </summary>
        /// <param name="input">格式: "字段:操作符:值"</param>
        /// <returns>SQL条件语句，无效输入返回null</returns>
        /// <remarks>
        /// 支持的操作符:
        /// <list type="bullet">
        /// <item>in: 包含字符串集合 (值用'|'分隔) 示例: status:in:active|pending</item>
        /// <item>out: 排除字符串集合 示例: status:out:deleted</item>
        /// <item>=: 包含数字集合 示例: id:=:1|2|3</item>
        /// <item>!=: 排除数字集合 示例: id:!=:4|5</item>
        /// <item>&lt;, &lt;=, &gt;, &gt;=: 单值比较 示例: age:&gt;=:18</item>
        /// <item>范围操作符(&lt;&gt;, &lt;=&gt;, &lt;&gt;=, &lt;=&gt;=): 范围比较 示例: price:&lt;&gt;:10~20</item>
        /// </list>
        /// </remarks>
        public static string ParseConditionOp(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // 分割输入为三部分 (字段:操作符:值)
            var segments = input.Split(':')
                                .Select(item => item.Trim())
                                .ToArray();

            // 验证基本格式
            if (segments.Length <= 2)
                return null;

            string field = segments[0];
            string op = segments[1].ToLowerInvariant(); // 使用不区分大小写的比较
            string value = segments[2];

            // 根据操作符类型路由处理逻辑
            switch (op)
            {
                case "in":
                    return $"{field} IN {ConvertToStringScope(value, '|')}";

                case "out":
                    return $"{field} NOT IN {ConvertToStringScope(value, '|')}";

                case "=":
                    return $"{field} IN {ConvertToNumScope(value, '|')}";

                case "!=":
                    return $"{field} NOT IN {ConvertToNumScope(value, '|')}";

                case "<":
                case "<=":
                case ">":
                case ">=":
                    return HandleSingleValueComparison(field, op, value);

                case "<>":
                case "<=>":
                case "<>=":
                case "<=>=":
                    return HandleRangeComparison(field, op, value);

                case "isnull":
                    return $"{field} IS NULL OR {field} = ''";
                case "nonull":
                    return $"{field} IS NOT NULL AND {field} <> ''";
                case "like":
                    return $"{field} LIKE '%{value}%'";
                default:
                    return null; // 不支持的操作符
            }
        }

        /// <summary>
        /// 处理单值比较操作符
        /// </summary>
        private static string HandleSingleValueComparison(string field, string op, string value)
        {
            var number = ParseNumber(value);
            return number != null ? $"{field} {op} {number}" : null;
        }

        /// <summary>
        /// 处理范围比较操作符
        /// </summary>
        private static string HandleRangeComparison(string field, string op, string rangeValue)
        {
            // 分割范围值并清理空项
            var rangeParts = rangeValue.Split('~')
                                      .Select(item => item.Trim())
                                      .Where(item => !string.IsNullOrWhiteSpace(item))
                                      .ToArray();

            // 范围值必须包含两个有效元素
            if (rangeParts.Length < 2)
                return null;

            // 解析边界值（取前两个有效值）
            var num1 = ParseNumber(rangeParts[0]);
            var num2 = ParseNumber(rangeParts[1]);

            if (num1 == null || num2 == null)
                return null;

            // 确定边界类型
            bool includeLower = op.Contains("=>") || op.EndsWith(">");
            bool includeUpper = op.Contains("<=") || op.StartsWith("<");

            string lowerOp = includeLower ? ">=" : ">";
            string upperOp = includeUpper ? "<=" : "<";

            // 自动确定最小/最大值
            double min = Math.Min(Convert.ToDouble(num1), Convert.ToDouble(num2));
            double max = Math.Max(Convert.ToDouble(num1), Convert.ToDouble(num2));

            return $"{field} {lowerOp} {min} AND {field} {upperOp} {max}";
        }

        /// <summary>
        /// 解析复合条件表达式
        /// </summary>
        /// <param name="input">由分号分隔的多个条件表达式，可使用'&'前缀指定AND连接</param>
        /// <returns>组合后的SQL条件语句，无效输入返回null</returns>
        /// <remarks>
        /// 格式说明：
        /// 1. 多个条件用分号(;)分隔
        /// 2. 条件前加'&'表示与前一个条件使用AND连接
        /// 3. 默认使用OR连接条件
        /// 
        /// 示例：
        ///   "age:>=:18; &status:in:active" 
        ///   → "age >= 18 OR status IN ('active')"
        ///   
        ///   "type:=:1|2; &status:in:active; &deleted:!=:1"
        ///   → "type IN (1,2) AND status IN ('active') AND deleted NOT IN (1)"
        /// </remarks>
        public static string ParseConditionExp(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // 分割并清理表达式
            var conditions = input.Split(';')
                                 .Select(item => item.Trim())
                                 .Where(item => !string.IsNullOrWhiteSpace(item))
                                 .ToList();

             var result = new StringBuilder();

           // 单个条件直接解析
            if (conditions.Count == 0)
                return null;
            if (conditions.Count == 1)
            {
                result.Append(ParseConditionOp(conditions[0]));
                return result.Length > 0 ? " where " + result.ToString() : null;
            }

            // 处理复合条件
            foreach (var condition in conditions)
            {
                // 检测AND连接符标记
                bool isAndCondition = condition.StartsWith("&");
                string cleanCondition = isAndCondition ? condition.Substring(1).Trim() : condition;

                // 解析当前条件
                string parsed = ParseConditionOp(cleanCondition);
                if (string.IsNullOrWhiteSpace(parsed))
                    continue;

                // 添加连接逻辑
                if (result.Length == 0)
                {
                    result.Append(parsed);
                }
                else
                {
                    result.Append(isAndCondition ? " AND " : " OR ");
                    result.Append(parsed);
                }

            }

            return result.Length > 0 ? " where " + result.ToString() : null;
        }      
        public static string ParseLinkExp(string input)
        {
            string linkWhereExp = string.Empty;
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // 分割并清理表达式
            var linkExp = input.Split(',')
                                 .Select(item => item.Trim())
                                 .ToList();

            // 单个条件直接解析
            if (linkExp.Count == 0)
                return null;
            if (linkExp.Count <= 1)
                return $"Select * From HK_{linkExp[0]} Order by SortNum";
            if (linkExp.Count == 3)
                return $"Select * From HK_{linkExp[0]} {ParseConditionExp(linkExp[2])}  Order by SortNum";
           return linkWhereExp;
        }

        /// <summary>
        /// 验证字符串是否符合数字项格式，由指定分隔符分隔
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <param name="separator">分隔符（默认 'x'）</param>
        /// <param name="expectedSeparatorCount">预期的分隔符数量（默认 0 表示不检查）</param>
        /// <returns>格式正确返回 true，否则返回 false</returns>
        public static bool ValidateNumberItemsFormat(
            string input,
            char separator = 'x',
            int expectedSeparatorCount = 0)
        {
            // 1. 允许空或空白字符串（直接返回true）
            if (string.IsNullOrWhiteSpace(input))
                return true;

            // 2. 分割字符串
            string[] parts = input.Split(separator);

            // 3. 根据分隔符数量验证部分数量
            if (expectedSeparatorCount > 0)
            {
                // 预期部分数 = 分隔符数量 + 1
                int expectedPartCount = expectedSeparatorCount + 1;

                if (parts.Length != expectedPartCount)
                    return false;
            }

            // 4. 验证每个部分是否是有效的正数
            foreach (string part in parts)
            {
                // 4.1 去除空格
                string trimmedPart = part.Trim();

                // 4.2 检查是否为空
                if (string.IsNullOrEmpty(trimmedPart))
                    return false;

                // 4.3 尝试解析为数字
                if (!double.TryParse(trimmedPart, out double value))
                    return false;

                // 4.4 检查是否为有效尺寸（大于0）
                if (value <= 0)
                    return false;
            }

            return true;
        }

        public static Func<T, object> CreateGetter<T>(string propertyName)
        {
            var param = Expression.Parameter(typeof(T));
            var prop = Expression.Property(param, propertyName);
            var convert = Expression.Convert(prop, typeof(object));
            return Expression.Lambda<Func<T, object>>(convert, param).Compile();
        }

    }
}
