namespace GPS.SettingsSync.Collections
{
    public interface IMapChangedEventArgs<K>
    {
        K Key { get; }
        DistributedCollectionChange DistributedCollectionChange { get; }
    }
}