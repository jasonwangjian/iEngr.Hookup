using iEngr.Hookup.Models;
using iEngr.Hookup.Services;
using iEngr.Hookup.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;

namespace iEngr.Hookup
{

    public static partial class HK_General
    {
        public static int ProjLanguage = 4; // 4: 中文； 2为英文
        public static readonly int RoleAE = 1; // 制图人
        public static readonly int RoleRE = 2; // 设计人
        public static readonly int RoleCK = 4; // 校对人
        public static readonly int RoleDL = 8; // 专业负责人
        public static readonly int RoleVF = 16; // 审核人
        public static readonly int RoleMG = 32; // 项目负责人
        public static readonly int RoleAP = 64; // 审定人
        public static readonly int RoleAdmin = 128; // 管理员

        public static bool IsAutoComosUpdate = true;
        public static readonly string[] portDef = { "EQ1", "DF1", "AS1", "NEQ" };
        public static readonly string[] portNA = { "NA", "IS" };

        public static UserComos UserComos = new UserComos(null) { RealName = "Anonymous",Roles = 255 };
        public static string UserName = UserComos.RealName;  //"Anonymous";

        public static string ErrMsgOmMatData {  get; set; }
        public static string GetPropertiesString(Dictionary<string, object> properties)
        {
            List<string> keyValues = new List<string>();
            foreach (var prop in properties)
            {
                string value = prop.Value?.ToString();
                if (prop.Value is ObservableCollection<GeneralItem> items)
                    value = string.Join("|", items.Select(x => x.Code).ToList());
                else if (prop.Value is GeneralItem item)
                    value = item?.Code;
                keyValues.Add(prop.Key + ":" + value);
            }
            return string.Join(",", keyValues);
        }
        public static string GetPropertiesString(ObservableCollection<PropertyDefinition> properties)
        {
            List<string> keyValues = new List<string>();
            foreach (var prop in properties)
            {
                string value = prop.Value?.ToString();
                if (prop.Type == PropertyType.EnumItems)
                    value = string.Join("|", prop.SelectedItems?.Select(x => x.Code).ToList());
                else if (prop.Type == PropertyType.EnumItem)
                    value = prop.SelectedItem?.Code;
                keyValues.Add(prop.Key + ":" + value);
            }
            return string.Join(",", keyValues);
        }
        public static string GetPropertyDisplay(string key, string value)
        {
            var propDef = PropertyLibrary.GetPropertyDefinition(key);
            string displayValue = null;
            if (propDef != null)
            {
                if (propDef.Type == PropertyType.EnumItems)
                {
                    displayValue = string.Join(", ", propDef.Items.Where(x => value.Split('|').Contains(x.Code)).Select(x=>x.Name));
                }
                else if (propDef.Type == PropertyType.EnumItem)
                {
                    displayValue = propDef.Items.FirstOrDefault(x => x.Code == value).Name;
                }
                else
                    displayValue = value;
            }
            return displayValue;
        }
        public static string GetPropertyDisplay(string key, object value)
        {
            var propDef = PropertyLibrary.GetPropertyDefinition(key);
            string displayValue = null;
            if (propDef != null)
            {
                if (value is ObservableCollection<GeneralItem> items)
                {
                    displayValue = string.Join(", ", items.Select(x => x.Name).ToList());
                }
                else if (value is GeneralItem gItem)
                {
                    displayValue = gItem.Name;
                }
                else
                    displayValue = value?.ToString();
            }
            return displayValue;
        }
        public static Dictionary<string, object> GetPropertyDictionary(string propertiesString)
        {
            Dictionary<string, object> propertyDictionary = new Dictionary<string, object>();
            var dicProp = new Dictionary<string, string>();
            propertiesString.Split(',')
                .Where(x => !string.IsNullOrWhiteSpace(x) && x.Contains(':'))
                .Select(x => x.Split(':', (char)2))
                .Where(parts => parts.Length == 2)
                .ToList()
                .ForEach(parts =>
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    if (!dicProp.ContainsKey(key))
                        dicProp.Add(key, value);
                });
            foreach (var prop in dicProp)
            {
                var propDef = PropertyLibrary.GetPropertyDefinition(prop.Key);
                if (propDef != null)
                {
                    if (propDef.Type == PropertyType.EnumItems)
                    {
                        propertyDictionary.Add(prop.Key, new ObservableCollection<GeneralItem>(propDef.Items.Where(x => prop.Value.Split('|').Contains(x.Code)).ToList()));
                    }
                    else if (propDef.Type == PropertyType.EnumItem)
                    {
                        propertyDictionary.Add(prop.Key, propDef.Items.FirstOrDefault(x => x.Code == prop.Value));
                    }
                    else
                        propertyDictionary.Add(prop.Key, prop.Value);
                }
            }
            return propertyDictionary;
        }
        public static ObservableCollection<LabelDisplay> GetPropLabelItems(HkTreeItem item)
        {
            //ObservableCollection<LabelDisplay> propLabelItems = new ObservableCollection<LabelDisplay>();
            //foreach (var prop in item?.Properties)
            //{
            //    var propDef = PropertyLibrary.GetPropertyDefinition(prop.Key);
            //    if (propDef != null)
            //    {
            //        string displayValue = prop.Value?.ToString();
            //        if (prop.Value is ObservableCollection<GeneralItem> items)
            //        {
            //            displayValue = string.Join(", ", items.Select(x => x.Name).ToList());
            //        }
            //        else if (prop.Value is GeneralItem gItem)
            //        {
            //            displayValue = gItem.Name;
            //        }
            //        propLabelItems.Add(new LabelDisplay
            //        {
            //            Key=prop.Key,
            //            SortNum = propDef.SortNum,
            //            DisplayName = propDef.DisplayName,
            //            DisplayValue1 = displayValue,
            //            IsNodeLabel = true,
            //            IsInherit = false
            //        });
            //    }
            //}
            //foreach (var prop in item?.InheritProperties)
            //{
            //    var propDef = PropertyLibrary.GetPropertyDefinition(prop.Key);
            //    if (propDef != null)
            //    {
            //        string displayValue = prop.Value?.ToString();
            //        if (prop.Value is ObservableCollection<GeneralItem> items)
            //        {
            //            displayValue = string.Join(", ", items.Select(x => x.Name).ToList());
            //        }
            //        else if (prop.Value is GeneralItem gItem)
            //        {
            //            displayValue = gItem.Name;
            //        }
            //        propLabelItems.Add(new LabelDisplay
            //        {
            //            Key = prop.Key,
            //            SortNum = propDef.SortNum,
            //            DisplayName = propDef.DisplayName,
            //            DisplayValue1 = displayValue,
            //            IsNodeLabel = true,
            //            IsInherit = true
            //        });
            //    }
            //}
            var propLabelItems = GetPropLabelItems(item?.Properties, true, false)
                                .Union(GetPropLabelItems(item?.InheritProperties, true, true));
            // 排序后返回新的 ObservableCollection
            return new ObservableCollection<LabelDisplay>(propLabelItems.OrderBy(x => x.SortNum));
        }
        public static ObservableCollection<LabelDisplay> GetPropLabelItems(DiagramItem item)
        {
            var properties = GetPropertyDictionary(item?.IdLabels);
            var propLabelItems = GetPropLabelItems(properties, false, false);
            // 排序后返回新的 ObservableCollection
            return new ObservableCollection<LabelDisplay>(propLabelItems.OrderBy(x => x.SortNum));
        }
        private static ObservableCollection<LabelDisplay> GetPropLabelItems(Dictionary<string,object> properties, bool isNodeLabel, bool isInherit)
        {
            ObservableCollection<LabelDisplay> propLabelItems = new ObservableCollection<LabelDisplay>();
            foreach (var prop in properties)
            {
                var propDef = PropertyLibrary.GetPropertyDefinition(prop.Key);
                if (propDef != null)
                {
                    string displayValue = prop.Value?.ToString();
                    if (prop.Value is ObservableCollection<GeneralItem> items)
                    {
                        displayValue = string.Join(", ", items.Select(x => x.Name).ToList());
                    }
                    else if (prop.Value is GeneralItem gItem)
                    {
                        displayValue = gItem.Name;
                    }
                    propLabelItems.Add(new LabelDisplay
                    {
                        Key = prop.Key,
                        SortNum = propDef.SortNum,
                        DisplayName = propDef.DisplayName,
                        DisplayValue1 = isNodeLabel ? displayValue : null,
                        DisplayValue2 = isNodeLabel ? null : displayValue,
                        IsNodeLabel = isNodeLabel,
                        IsComosLabel = !isNodeLabel,
                        IsInherit = isInherit
                    });
                }
            }
            return new ObservableCollection<LabelDisplay>(propLabelItems.OrderBy(x => x.SortNum));
        }
        private static void AddPropLabelItems(ObservableCollection<LabelDisplay> propLabelItems, Dictionary<string, object> properties, bool isNodeLabel, bool isInherit)
        {
            foreach (var prop in properties)
            {
                var propDef = PropertyLibrary.GetPropertyDefinition(prop.Key);
                if (propDef != null)
                {
                    string displayValue = prop.Value?.ToString();
                    if (prop.Value is ObservableCollection<GeneralItem> items)
                    {
                        displayValue = string.Join(", ", items.Select(x => x.Name).ToList());
                    }
                    else if (prop.Value is GeneralItem gItem)
                    {
                        displayValue = gItem.Name;
                    }
                    propLabelItems.Add(new LabelDisplay
                    {
                        Key = prop.Key,
                        SortNum = propDef.SortNum,
                        DisplayName = propDef.DisplayName,
                        DisplayValue1 = displayValue,
                        IsNodeLabel = isNodeLabel,
                        IsInherit = isInherit
                    });
                }
            }
        }
    }
}
