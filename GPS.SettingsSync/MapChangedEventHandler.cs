namespace GPS.SettingsSync
{
    public delegate void MapChangedEventHandler<K, V>(IObservableMap<K, V> sender, IMapChangedEventArgs<K> args);
}