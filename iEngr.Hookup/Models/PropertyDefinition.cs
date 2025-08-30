using System.Collections.ObjectModel;

namespace iEngr.Hookup.Models
{
    public class PropertyDefinition
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public PropertyType Type { get; set; }
        public string Category { get; set; }
        public object DefaultValue { get; set; }
        public object Value { get; set; }
        public ObservableCollection<object> Options { get; set; }
    }

    public enum PropertyType
    {
        String,
        Integer,
        Double,
        Boolean,
        DateTime,
        Enum
    }
}