using System;
using System.Collections.Concurrent;

namespace GPS.SettingsSync.Collections
{
    public sealed class DistributedPropertySet : ConcurrentDictionary<string, object>, IDistributedPropertySet
    {
        public new bool TryAdd(string key, object value)
        {
            if (!base.TryAdd(key, value)) return false;

            MapChanged?.Invoke(this, new MapChangedEventArgs<string>(key, DistributedCollectionChange.ItemInserted));

            return true;
        }

        public new object AddOrUpdate(string key, object addValue, Func<string, object, object> updateFactory)
        {
            if (ContainsKey(key))
            {
                TryUpdate(key, updateFactory?.Invoke(key, this[key]), this[key]);
            }
            else
            {
                TryAdd(key, addValue);
            }

            return TryGetValue(key, out var value) ? value : null;
        }

        public new object AddOrUpdate(string key, Func<string, object> addFactory, Func<string, object, object> updateFactory)
        {
            if (ContainsKey(key))
            {
                TryUpdate(key, updateFactory?.Invoke(key, this[key]), this[key]);
            }
            else
            {
                TryAdd(key, addFactory?.Invoke(key));
            }

            return TryGetValue(key, out var value) ? value : null;
        }

        public new object GetOrAdd(string key, object value)
        {
            if (ContainsKey(key))
            {
                return this[key];
            }

            return TryAdd(key, value) ? value : null;
        }

        public new object GetOrAdd(string key, Func<string, object> valueFactory)
        {
            if (ContainsKey(key))
            {
                return this[key];
            }

            return TryAdd(key, valueFactory?.Invoke(key)) ? this[key] : null;
        }

        public object GetOrAdd<TArg>(string key, Func<string, TArg, object> valueFactory, TArg arg)
        {
            if (ContainsKey(key))
            {
                return this[key];
            }

            return TryAdd(key, valueFactory?.Invoke(key, arg)) ? this[key] : null;
        }

        public new bool TryRemove(string key, out object value)
        {
            if (!base.TryRemove(key, out value)) return false;

            MapChanged?.Invoke(this, new MapChangedEventArgs<string>(key, DistributedCollectionChange.ItemRemoved));

            return true;
        }

        public new bool TryUpdate(string key, object newValue, object comparisonValue)
        {
            if (!base.TryUpdate(key, newValue, comparisonValue)) return false;

            MapChanged?.Invoke(this, new MapChangedEventArgs<string>(key, DistributedCollectionChange.ItemChanged));

            return true;
        }

        public new void Clear()
        {
            base.Clear();

            MapChanged?.Invoke(this, new MapChangedEventArgs<string>((string)null, DistributedCollectionChange.Reset));
        }

        public event MapChangedEventHandler<string, object> MapChanged;
    }
}