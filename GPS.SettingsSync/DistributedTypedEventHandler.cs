namespace GPS.SettingsSync
{
    public delegate void DistributedTypedEventHandler<TSender, TResult>(TSender sender, TResult args);
}