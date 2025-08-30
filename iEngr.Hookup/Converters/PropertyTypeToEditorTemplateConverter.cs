using iEngr.Hookup.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace iEngr.Hookup.Converters
{
    public class PropertyTypeToEditorTemplateConverter : IValueConverter
    {
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate IntegerTemplate { get; set; }
        public DataTemplate DoubleTemplate { get; set; }
        public DataTemplate BooleanTemplate { get; set; }
        public DataTemplate EnumTemplate { get; set; }
        public DataTemplate DateTimeTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PropertyType type)
            {
                return type switch
                {
                    PropertyType.String => StringTemplate,
                    PropertyType.Integer => IntegerTemplate,
                    PropertyType.Double => DoubleTemplate,
                    PropertyType.Boolean => BooleanTemplate,
                    PropertyType.Enum => EnumTemplate,
                    PropertyType.DateTime => DateTimeTemplate,
                    _ => StringTemplate
                };
            }
            return StringTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}