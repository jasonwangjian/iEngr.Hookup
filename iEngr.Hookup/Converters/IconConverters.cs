using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace iEngr.Hookup.Converters
{
    public static class IconResourceHelper
    {
        public static readonly DependencyProperty ResourceKeyProperty =
            DependencyProperty.RegisterAttached("ResourceKey", typeof(string), typeof(IconResourceHelper),
                new PropertyMetadata(null, OnResourceKeyChanged));

        public static string GetResourceKey(DependencyObject obj)
        {
            return (string)obj.GetValue(ResourceKeyProperty);
        }

        public static void SetResourceKey(DependencyObject obj, string value)
        {
            obj.SetValue(ResourceKeyProperty, value);
        }

        private static void OnResourceKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Image image && e.NewValue is string resourceKey)
            {
                resourceKey = $"{resourceKey.Trim()}Icon";
                // 查找资源
                var resource = image.TryFindResource(resourceKey) ?? Application.Current.TryFindResource(resourceKey)
                    ?? image.TryFindResource("DefaultIcon") ?? Application.Current.TryFindResource("DefaultIcon");
                image.Source = resource as ImageSource;
            }
        }
    }
    public class StringToIconResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string iconName)
            {
                // 构建资源键名
                string resourceKey = $"{iconName.Trim()}Icon";

                // 在应用程序资源中查找
                var resource = Application.Current.TryFindResource(resourceKey);

                if (resource != null)
                {
                    return resource;
                }

                // 如果找不到特定国家的图标，返回默认图标
                return System.Windows.Application.Current.TryFindResource("DefaultIcon")
                       ?? new BitmapImage(new Uri("pack://application:,,,/iEngr.Hookup;component/Resources/DefaultIcon.ico"));
            }

            return System.Windows.Application.Current.TryFindResource("DefaultIcon")
                   ?? new BitmapImage(new Uri("pack://application:,,,/iEngr.Hookup;component/Resources/DefaultIcon.ico"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
