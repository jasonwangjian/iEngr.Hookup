using iEngr.Hookup.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace iEngr.Hookup.Converters
{
    public class PropertyTypeToEditorTemplateConverter : IValueConverter
    {
        private DataTemplate _stringTemplate;
        private DataTemplate _integerTemplate;
        private DataTemplate _doubleTemplate;
        private DataTemplate _booleanTemplate;
        private DataTemplate _enumTemplate;
        private DataTemplate _dateTimeTemplate;

        public DataTemplate StringTemplate
        {
            get => _stringTemplate;
            set => _stringTemplate = value;
        }

        public DataTemplate IntegerTemplate
        {
            get => _integerTemplate;
            set => _integerTemplate = value;
        }

        public DataTemplate DoubleTemplate
        {
            get => _doubleTemplate;
            set => _doubleTemplate = value;
        }

        public DataTemplate BooleanTemplate
        {
            get => _booleanTemplate;
            set => _booleanTemplate = value;
        }

        public DataTemplate EnumTemplate
        {
            get => _enumTemplate;
            set => _enumTemplate = value;
        }

        public DataTemplate DateTimeTemplate
        {
            get => _dateTimeTemplate;
            set => _dateTimeTemplate = value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return StringTemplate;

            if (value is PropertyType propertyType)
            {
                switch (propertyType)
                {
                    case PropertyType.String:
                        return StringTemplate ?? throw new InvalidOperationException("StringTemplate is not set");
                    case PropertyType.Integer:
                        return IntegerTemplate ?? throw new InvalidOperationException("IntegerTemplate is not set");
                    case PropertyType.Double:
                        return DoubleTemplate ?? throw new InvalidOperationException("DoubleTemplate is not set");
                    case PropertyType.Boolean:
                        return BooleanTemplate ?? throw new InvalidOperationException("BooleanTemplate is not set");
                    case PropertyType.Enum:
                        return EnumTemplate ?? throw new InvalidOperationException("EnumTemplate is not set");
                    case PropertyType.DateTime:
                        return DateTimeTemplate ?? throw new InvalidOperationException("DateTimeTemplate is not set");
                    default:
                        return StringTemplate ?? throw new InvalidOperationException("StringTemplate is not set");
                }
            }

            return StringTemplate ?? throw new InvalidOperationException("StringTemplate is not set");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("PropertyTypeToEditorTemplateConverter only supports one-way conversion");
        }
    }
}