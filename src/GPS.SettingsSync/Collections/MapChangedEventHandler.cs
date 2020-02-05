namespace GPS.SettingsSync.Core.Collections
{
    public delegate void MapChangedEventHandler<K, V>(IObservableMap<K, V> sender, IMapChangedEventArgs<K> args);
}