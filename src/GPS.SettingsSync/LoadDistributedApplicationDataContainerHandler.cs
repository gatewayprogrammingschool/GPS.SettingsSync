namespace GPS.SettingsSync.Core
{
    public delegate void LoadDistributedApplicationDataContainerHandler(string name,
        DistributedApplicationDataLocality locality,
        DistributedApplicationDataContainer container);
}