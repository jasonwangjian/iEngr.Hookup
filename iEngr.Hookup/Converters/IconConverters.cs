using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace iEngr.Hookup.Converters
{
    public class CountryToIconResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string countryCode)
            {
                // 构建资源键名
                string resourceKey = $"{countryCode}Icon";

                // 在应用程序资源中查找
                var resource = System.Windows.Application.Current.TryFindResource(resourceKey);

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
