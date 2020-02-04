namespace GPS.SettingsSync
{
    public sealed class DistributedSetVersionRequest
    {
        public DistributedSetVersionRequest(uint currentVersion, uint desiredVersion)
        {
            CurrentVersion = currentVersion;
            DesiredVersion = desiredVersion;
        }

        public uint CurrentVersion { get; }
        public uint DesiredVersion { get; }

        public DistributedSetVersionDeferral GetDeferral()
        {
            return new DistributedSetVersionDeferral();
        }
    }
}