using iEngr.Hookup.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace iEngr.Hookup.Services
{
    public static class PropertyLibrary
    {
        public static ObservableCollection<PropertyDefinition> AllProperties { get; } = new ObservableCollection<PropertyDefinition>
        {
            // 基础信息
            new PropertyDefinition { Key = "Description", DisplayName = "描述", Type = PropertyType.String, Category = "基本信息" },
            new PropertyDefinition { Key = "Code", DisplayName = "代码", Type = PropertyType.String, Category = "基本信息" },
            
            // 地理信息
            new PropertyDefinition { Key = "Country", DisplayName = "国家", Type = PropertyType.String, Category = "地理信息" },
            new PropertyDefinition { Key = "City", DisplayName = "城市", Type = PropertyType.String, Category = "地理信息" },
            new PropertyDefinition { Key = "Address", DisplayName = "地址", Type = PropertyType.String, Category = "地理信息" },
            new PropertyDefinition { Key = "Latitude", DisplayName = "纬度", Type = PropertyType.Double, Category = "地理信息" },
            new PropertyDefinition { Key = "Longitude", DisplayName = "经度", Type = PropertyType.Double, Category = "地理信息" },
            
            // 数字信息
            new PropertyDefinition { Key = "Population", DisplayName = "人口", Type = PropertyType.Integer, Category = "统计信息" },
            new PropertyDefinition { Key = "Area", DisplayName = "面积", Type = PropertyType.Double, Category = "统计信息" },
            new PropertyDefinition { Key = "Elevation", DisplayName = "海拔", Type = PropertyType.Integer, Category = "统计信息" },
            new PropertyDefinition { Key = "Density", DisplayName = "密度", Type = PropertyType.Double, Category = "统计信息" },
            
            // 状态信息
            new PropertyDefinition { Key = "Status", DisplayName = "状态", Type = PropertyType.Enum, Category = "状态信息",
                Options = new ObservableCollection<object> { "Active", "Inactive", "Pending", "Completed" } },
            new PropertyDefinition { Key = "Priority", DisplayName = "优先级", Type = PropertyType.Enum, Category = "状态信息",
                Options = new ObservableCollection<object> { "High", "Medium", "Low" } },
            new PropertyDefinition { Key = "IsEnabled", DisplayName = "是否启用", Type = PropertyType.Boolean, Category = "状态信息", DefaultValue = true },
            
            // 时间信息
            new PropertyDefinition { Key = "CreatedDate", DisplayName = "创建时间", Type = PropertyType.DateTime, Category = "时间信息" },
            new PropertyDefinition { Key = "ModifiedDate", DisplayName = "修改时间", Type = PropertyType.DateTime, Category = "时间信息" },
            new PropertyDefinition { Key = "ExpiryDate", DisplayName = "过期时间", Type = PropertyType.DateTime, Category = "时间信息" },
            
            // 联系信息
            new PropertyDefinition { Key = "Phone", DisplayName = "电话", Type = PropertyType.String, Category = "联系信息" },
            new PropertyDefinition { Key = "Email", DisplayName = "邮箱", Type = PropertyType.String, Category = "联系信息" },
            new PropertyDefinition { Key = "Website", DisplayName = "网站", Type = PropertyType.String, Category = "联系信息" },
            
            // 可以继续添加更多属性...
        };

        public static PropertyDefinition GetPropertyDefinition(string key)
        {
            return AllProperties.FirstOrDefault(p => p.Key == key);
        }

        public static ObservableCollection<PropertyDefinition> GetPropertiesByCategory(string category)
        {
            return new ObservableCollection<PropertyDefinition>(AllProperties.Where(p => p.Category == category));
        }
    }
}