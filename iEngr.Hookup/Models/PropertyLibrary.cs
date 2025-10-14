using iEngr.Hookup.Models;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace iEngr.Hookup.Services
{
    public static class PropertyLibrary
    {
        //public static ObservableCollection<PropertyDefinition> AllProperties { get; } = new ObservableCollection<PropertyDefinition>
        //{
        //    // Specifications
        //    new PropertyDefinition { Key = "ConnType", DisplayNameCn = "连接方式", Type = PropertyType.EnumItem, Category = "Spec",
        //                  Items= new ObservableCollection<GeneralItem>(HK_General.dicPortType.Select(x=>x.Value).Select(x=> new GeneralItem
        //                  {
        //                      Code = x.ID,
        //                      NameCn =x.NameCn,
        //                      NameEn=x.NameEn

        //                  }).ToList())},

        //     new PropertyDefinition { Key = "ConnTypes", DisplayNameCn = "各种连接方式", Type = PropertyType.EnumItems, Category = "Spec",
        //                  Items= new ObservableCollection<GeneralItem>(HK_General.dicPortType.Select(x=>x.Value).Select(x=> new GeneralItem
        //                  {
        //                      Code = x.ID,
        //                      NameCn =x.NameCn,
        //                      NameEn=x.NameEn
        //                  }).ToList())},

        //    // CDevice标签
        //    // 基础信息
        //    new PropertyDefinition { Key = "Description", DisplayNameCn = "描述", Type = PropertyType.String, Category = "基本信息" },
        //    new PropertyDefinition { Key = "Code", DisplayNameCn = "代码", Type = PropertyType.String, Category = "基本信息" },

        //    // 地理信息
        //    new PropertyDefinition { Key = "Country", DisplayNameCn = "国家", Type = PropertyType.String, Category = "地理信息" },
        //    new PropertyDefinition { Key = "City", DisplayNameCn = "城市", Type = PropertyType.String, Category = "地理信息" },
        //    new PropertyDefinition { Key = "Address", DisplayNameCn = "地址", Type = PropertyType.String, Category = "地理信息" },
        //    new PropertyDefinition { Key = "Latitude", DisplayNameCn = "纬度", Type = PropertyType.Double, Category = "地理信息" },
        //    new PropertyDefinition { Key = "Longitude", DisplayNameCn = "经度", Type = PropertyType.Double, Category = "地理信息" },

        //    // 数字信息
        //    new PropertyDefinition { Key = "Population", DisplayNameCn = "人口", Type = PropertyType.Integer, Category = "统计信息" },
        //    new PropertyDefinition { Key = "Area", DisplayNameCn = "面积", Type = PropertyType.Double, Category = "统计信息" },
        //    new PropertyDefinition { Key = "Elevation", DisplayNameCn = "海拔", Type = PropertyType.Integer, Category = "统计信息" },
        //    new PropertyDefinition { Key = "Density", DisplayNameCn = "密度", Type = PropertyType.Double, Category = "统计信息" },

        //    // 状态信息
        //    new PropertyDefinition { Key = "Status", DisplayNameCn = "状态", Type = PropertyType.Enum, Category = "状态信息",
        //        Options = new ObservableCollection<object> { "Active", "Inactive", "Pending", "Completed" } },
        //    new PropertyDefinition { Key = "Priority", DisplayNameCn = "优先级", Type = PropertyType.Enum, Category = "状态信息",
        //        Options = new ObservableCollection<object> { "High", "Medium", "Low" } },
        //    new PropertyDefinition { Key = "IsEnabled", DisplayNameCn = "是否启用", Type = PropertyType.Boolean, Category = "状态信息", DefaultValue = true },

        //    // 时间信息
        //    new PropertyDefinition { Key = "CreatedDate", DisplayNameCn = "创建时间", Type = PropertyType.DateTime, Category = "时间信息" },
        //    new PropertyDefinition { Key = "ModifiedDate", DisplayNameCn = "修改时间", Type = PropertyType.DateTime, Category = "时间信息" },
        //    new PropertyDefinition { Key = "ExpiryDate", DisplayNameCn = "过期时间", Type = PropertyType.DateTime, Category = "时间信息" },

        //    // 联系信息
        //    new PropertyDefinition { Key = "Phone", DisplayNameCn = "电话", Type = PropertyType.String, Category = "联系信息" },
        //    new PropertyDefinition { Key = "Email", DisplayNameCn = "邮箱", Type = PropertyType.String, Category = "联系信息" },
        //    new PropertyDefinition { Key = "Website", DisplayNameCn = "网站", Type = PropertyType.String, Category = "联系信息" },

        //    // 可以继续添加更多属性...
        //};
        public static ObservableCollection<PropertyDefinition> AllProperties = AllPropertiesIni();
        private static ObservableCollection<PropertyDefinition> AllPropertiesIni()
        {
            ObservableCollection<PropertyDefinition>  allProperties = new ObservableCollection<PropertyDefinition>();
            // Device标签
            foreach (string key in HK_General.dicDevLabel.Keys)
            {
                HKLibDevLabel devLabel = HK_General.dicDevLabel[key];
                allProperties.Add(new PropertyDefinition()
                {
                    Key = devLabel.ID,
                    DisplayNameCn = devLabel.NameCn,
                    DisplayNameEn = devLabel.NameEn,
                    RemarksCn = devLabel.RemarksCn,
                    RemarksEn = devLabel.RemarksEn,
                    Type = PropertyType.EnumItems,
                    Category = "CDevice",
                    Items = new ObservableCollection<GeneralItem>(HK_General.dicDevValue.Where(x => x.Value.DevTag == devLabel.ID).Select(x => x.Value).Select(x => new GeneralItem
                    {
                        Code = x.ID,
                        NameCn = x.NameCn,
                        NameEn = x.NameEn

                    }).ToList())
                });
            }
            return allProperties;
        }

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