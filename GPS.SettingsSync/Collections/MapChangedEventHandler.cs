namespace GPS.SettingsSync.Collections
{
    public delegate void MapChangedEventHandler<K, V>(IObservableMap<K, V> sender, IMapChangedEventArgs<K> args);
}