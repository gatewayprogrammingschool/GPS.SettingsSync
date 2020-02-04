using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GPS.SettingsSync.Collections;

namespace GPS.SettingsSync
{
    public sealed class DistributedApplicationData
    {
        private static DistributedApplicationData _current;
        private DistributedApplicationDataContainer _localSettings;
        private DistributedApplicationDataContainer _roamingSettings;

        private DistributedApplicationData()
        {
            LocalSettings.Values.MapChanged += ValuesOnMapChanged;
            RoamingSettings.Values.MapChanged += ValuesOnMapChanged;
        }

        private void ValuesOnMapChanged(IObservableMap<string, object> sender, IMapChangedEventArgs<string> args)
        {
            DataChanged?.Invoke(this, sender);
        }

        public static DistributedApplicationData Current => _current ??= new DistributedApplicationData();

        public DistributedApplicationDataContainer LocalSettings => _localSettings ??= 
            new DistributedApplicationDataContainer().CreateContainer("Local", DistributedApplicationDataLocality.Local);

        public DistributedApplicationDataContainer RoamingSettings =>
            _roamingSettings ??= new DistributedApplicationDataContainer().CreateContainer("Roaming",
                DistributedApplicationDataLocality.Roaming);

        public uint Version { get; internal set; }

        public Task ClearAsync()
        {
            return Task.Run(() =>
            {
                LocalSettings.Values.Clear();
                LocalSettings.Containers.Values.ToList().ForEach(container => container.Values.Clear());
                RoamingSettings.Values.Clear();
                RoamingSettings.Containers.Values.ToList().ForEach(container => container.Values.Clear());
            });
        }

        public static Task<DistributedApplicationData> GetForUserAsync(string user)
        {
            return Task.FromResult(Current);
        }

        public Task SetVersionAsync(uint desiredVersion, DistributedApplicationDataSetVersionHandler handler)
        {
            return Task.Run(() => handler?.Invoke(new DistributedSetVersionRequest(Version, Version + 1)));
        }

        public void SignalDataChanged()
        {
            DataChanged?.Invoke(this, _roamingSettings.Values);
        }

        public event DistributedTypedEventHandler<DistributedApplicationData, IObservableMap<string, object>> DataChanged;
    }
}