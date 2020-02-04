using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GPS.SettingsSync.Collections;

namespace GPS.SettingsSync
{
    public class DistributedApplicationDataContainer
    {
        private ConcurrentDictionary<string, DistributedApplicationDataContainer> _containers;
        private DistributedPropertySet _values;

        public IReadOnlyDictionary<string, DistributedApplicationDataContainer> Containers => _containers ??= 
            new ConcurrentDictionary<string, DistributedApplicationDataContainer>();

        public DistributedApplicationDataLocality Locality { get; }

        public string Name { get; }

        public IDistributedPropertySet Values => _values ??= new DistributedPropertySet();

        internal DistributedApplicationDataContainer(
            string name = "Temporary",
            DistributedApplicationDataLocality locality = DistributedApplicationDataLocality.Temporary)
        {
            Name = name;
            Locality = locality;

            LoadContainer?.Invoke(name, locality, this);
        }

        public DistributedApplicationDataContainer CreateContainer(
            string name,
            DistributedApplicationDataLocality locality)
        {
            return Containers.ContainsKey(name) 
                ? Containers[name] : 
                _containers.TryAdd(name, new DistributedApplicationDataContainer(name, locality)) 
                    ? Containers[name] 
                    : null;
        }

        public void DeleteContainer(string name)
        {
            if (!_containers.ContainsKey(name)) return;

            _containers.TryRemove(name, out var container);

            RemoveContainer?.Invoke(this, container);
        }

        public event
            DistributedTypedEventHandler<DistributedApplicationDataContainer, DistributedApplicationDataContainer>
            RemoveContainer;

        public event
            LoadDistributedApplicationDataContainerHandler 
            LoadContainer;
    }
}
