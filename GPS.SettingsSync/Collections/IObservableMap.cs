using System.Collections.Generic;

namespace GPS.SettingsSync.Collections
{
    public interface IObservableMap<K, V> : IDictionary<K, V>
    {
        event MapChangedEventHandler<K, V> MapChanged;
    }
}