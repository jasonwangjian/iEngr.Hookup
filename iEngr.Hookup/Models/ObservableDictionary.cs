using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace iEngr.Hookup.Models
{
    public class ObservableDictionary<TKey, TValue> : INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly ObservableCollection<KeyValuePair<TKey, TValue>> _items;
        private readonly Dictionary<TKey, TValue> _dictionary;

        public ObservableDictionary()
        {
            _items = new ObservableCollection<KeyValuePair<TKey, TValue>>();
            _dictionary = new Dictionary<TKey, TValue>();
            _items.CollectionChanged += (s, e) => CollectionChanged?.Invoke(this, e);
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            _items.Add(new KeyValuePair<TKey, TValue>(key, value));
            OnPropertyChanged(nameof(Count));
        }

        public bool Remove(TKey key)
        {
            if (_dictionary.Remove(key))
            {
                var item = _items.FirstOrDefault(x => x.Key.Equals(key));
                _items.Remove(item);
                OnPropertyChanged(nameof(Count));
                return true;
            }
            return false;
        }

        public void Clear()
        {
            _dictionary.Clear();
            _items.Clear();
            OnPropertyChanged(nameof(Count));
        }

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                if (_dictionary.ContainsKey(key))
                {
                    _dictionary[key] = value;
                    var index = _items.ToList().FindIndex(x => x.Key.Equals(key));
                    if (index >= 0)
                    {
                        _items[index] = new KeyValuePair<TKey, TValue>(key, value);
                    }
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public int Count => _dictionary.Count;
        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        public IEnumerable<KeyValuePair<TKey, TValue>> Items => _items;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}