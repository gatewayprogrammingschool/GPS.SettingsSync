namespace GPS.SettingsSync
{
    public interface IMapChangedEventArgs<K>
    {
        K Key { get; }
        DistributedCollectionChange DistributedCollectionChange { get; }
    }
}