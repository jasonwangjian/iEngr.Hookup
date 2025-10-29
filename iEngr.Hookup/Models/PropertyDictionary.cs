using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup.Models
{
    public class PropertyDictionary : INotifyPropertyChanged
    {
        private Dictionary<string, object> _properties = new Dictionary<string, object>();
        public Dictionary<string, object> Properties
        {
            get => _properties;
            set => SetField(ref _properties, value);
        }
        // 获取属性值
        public object GetProperty(string key)
        {
            return _properties.ContainsKey(key) ? _properties[key] : null;
        }
        // 设置属性值
        public void SetProperty(string key, object value)
        {
            if (_properties.ContainsKey(key))
            {
                _properties[key] = value;
            }
            else
            {
                _properties.Add(key, value);
            }
            OnPropertyChanged(nameof(Properties));
        }
        // 检查是否已选择某个属性
        public bool HasProperty(string key)
        {
            return Properties.ContainsKey(key);
        }
        // 移除属性
        public void RemoveProperty(string key)
        {
            if (_properties.ContainsKey(key))
            {
                _properties.Remove(key);
            }
            OnPropertyChanged(nameof(Properties));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
