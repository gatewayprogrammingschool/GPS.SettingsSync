using System.Collections.Generic;

namespace GPS.SettingsSync.Core.Collections
{
    public interface IObservableMap<K, V> : IDictionary<K, V>
    {
        event MapChangedEventHandler<K, V> MapChanged;
    }
}