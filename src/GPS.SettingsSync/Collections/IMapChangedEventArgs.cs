namespace GPS.SettingsSync.Core.Collections
{
    public interface IMapChangedEventArgs<K>
    {
        K Key { get; }
        DistributedCollectionChange DistributedCollectionChange { get; }
    }
}