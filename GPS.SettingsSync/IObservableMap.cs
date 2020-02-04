using System.Collections.Generic;

namespace GPS.SettingsSync
{
    public interface IObservableMap<K, V> : IDictionary<K, V>
    {
        event MapChangedEventHandler<K, V> MapChanged;
    }
}