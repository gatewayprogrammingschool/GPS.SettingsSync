namespace GPS.SettingsSync.Collections
{
    public class MapChangedEventArgs<K> : IMapChangedEventArgs<K>
    {
        public MapChangedEventArgs(K key, DistributedCollectionChange distributedCollectionChange)
        {
            Key = key;
            DistributedCollectionChange = distributedCollectionChange;
        }

        public K Key { get; }
        public DistributedCollectionChange DistributedCollectionChange { get; }
    }
}