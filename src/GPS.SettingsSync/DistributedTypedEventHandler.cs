namespace GPS.SettingsSync.Core
{
    public delegate void DistributedTypedEventHandler<TSender, TResult>(TSender sender, TResult args);
}