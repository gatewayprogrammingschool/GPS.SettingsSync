using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GPS.SettingsSync.Core.Collections;

namespace GPS.SettingsSync.Core
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
            var containers = (new[] {LocalSettings, RoamingSettings}).SelectMany(container => container.Containers.Values).ToList();
            containers.Add(_localSettings); 
            containers.Add(_roamingSettings);
            DataChanged?.Invoke(this, containers.FirstOrDefault(container => container.Values == sender));
        }

        public static DistributedApplicationData Current => _current ??= new DistributedApplicationData();

        public DistributedApplicationDataContainer LocalSettings => _localSettings ??= 
            new DistributedApplicationDataContainer().CreateContainer(Name, DistributedApplicationDataLocality.Local);

        public DistributedApplicationDataContainer RoamingSettings =>
            _roamingSettings ??= new DistributedApplicationDataContainer().CreateContainer(Name,
                DistributedApplicationDataLocality.Roaming);

        private string _name;
        
        public string Name
        {
            get => _name ??= Assembly.GetEntryAssembly()?.GetName().Name;
            set => _name = value;
        }
        public uint Version { get; internal set; }

        public void Clear()
        {
            LocalSettings.Values.Clear();
            
            var parallelList = LocalSettings.Containers.Values.AsParallel();
            parallelList.ForAll(container => container.Values.Clear());
            
            RoamingSettings.Values.Clear();
            var parallelListRoaming = RoamingSettings.Containers.Values.AsParallel();
            parallelListRoaming.ForAll(container => container.Values.Clear());
        }
        
        public Task SetVersionAsync(uint desiredVersion, DistributedApplicationDataSetVersionHandler handler)
        {
            return Task.Run(() => handler?.Invoke(new DistributedSetVersionRequest(Version, Version + 1)));
        }
        
        public event DistributedTypedEventHandler<DistributedApplicationData, DistributedApplicationDataContainer> DataChanged;
    }
}