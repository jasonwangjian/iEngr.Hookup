// PropertyEditItem.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using iEngr.Hookup.Models;

namespace iEngr.Hookup.ViewModels
{
    public class PropertyEditItem : INotifyPropertyChanged
    {
        private object _value;

        public PropertyDefinition Definition { get; }

        public object Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        public PropertyEditItem(PropertyDefinition definition, object initialValue = null)
        {
            Definition = definition;
            Value = initialValue ?? definition.DefaultValue;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}