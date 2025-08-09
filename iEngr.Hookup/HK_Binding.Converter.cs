using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

namespace iEngr.Hookup
{
    /// <summary>
    /// 转换器
    /// </summary>
    public class PortVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // 检查 Items 是否为空
            if (((values[0] is int count) ? count : 0) > 0)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed; // 或者 Visibility.Hidden
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class SpecTVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // 第一个值是 Items.Count
            int itemCount = (values[0] is int count) ? count : 0;

            // 第二个值是 SelectedItem
            object selectedItem = values[1];

            // 条件判断
            bool isVisible = itemCount > 0 &&
                           selectedItem != null &&
                           (selectedItem as HKLibSpecDic).ID != "-";

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class DisabledIndexBindingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] = ComboBox2.IsEnabled (bool)
            // values[1] = ComboBox1.SelectedIndex (int)
            if (values.Length >= 2 && values[0] is bool isEnabled && !isEnabled)
            {
                return values[1]; // 返回 ComboBox1 的索引
            }
            return -1; // 默认值（当启用时）
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
