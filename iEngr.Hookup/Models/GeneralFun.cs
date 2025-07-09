using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup.Models
{
    public static class GeneralFun
    {
        public static string ConvertToSqlInString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "()";

            // 过滤特殊字符（防止SQL注入）
            var safeItems = input.Split(',')
                                 .Select(item => item.Trim())
                                 .Where(item => !string.IsNullOrWhiteSpace(item))
                                 .Select(item => item.Replace("'", "''")) // 处理单引号
                                 .ToList();

            if (!safeItems.Any())
                return "()";

            return $"('{string.Join("','", safeItems)}')";
        }
    }
}
