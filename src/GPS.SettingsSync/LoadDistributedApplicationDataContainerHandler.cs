namespace GPS.SettingsSync
{
    public delegate void LoadDistributedApplicationDataContainerHandler(string name,
        DistributedApplicationDataLocality locality,
        DistributedApplicationDataContainer container);
}