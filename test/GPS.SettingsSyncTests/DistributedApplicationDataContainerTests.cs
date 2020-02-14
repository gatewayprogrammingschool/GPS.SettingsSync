using System.Reflection;
using GPS.SettingsSync.Core;
using Xunit;

namespace GPS.SettingsSyncTests
{
    public class DistributedApplicationDataContainerTests
    {
        [Fact()]
        public void CreateContainerTest_LocalSettings()
        {
            var container = DistributedApplicationData.Current.LocalSettings;

            Assert.NotNull(container);
            Assert.Equal(Assembly.GetEntryAssembly()?.GetName().Name, container.Name);
            Assert.Equal(DistributedApplicationDataLocality.Local, container.Locality);
            Assert.NotNull(container.Values);
        }

        [Fact()]
        public void CreateContainerTest_RoamingSettings()
        {
            var container = DistributedApplicationData.Current.RoamingSettings;

            Assert.NotNull(container);
            Assert.Equal(Assembly.GetEntryAssembly()?.GetName().Name, container.Name);
            Assert.Equal(DistributedApplicationDataLocality.Roaming, container.Locality);
            Assert.NotNull(container.Values);
        }

        [Fact()]
        public void CreateContainerTest_TemporarySettings()
        {
            var container = DistributedApplicationData.Current.LocalSettings.CreateContainer("Temporary", DistributedApplicationDataLocality.Temporary);

            Assert.NotNull(container);
            Assert.Equal("Temporary", container.Name);
            Assert.Equal(DistributedApplicationDataLocality.Temporary, container.Locality);
            Assert.NotNull(container.Values);
            Assert.Contains(DistributedApplicationData.Current.LocalSettings.Containers.Keys, s => s == "Temporary");
        }

        [Fact()]
        public void DeleteContainerTest()
        {
            var container = DistributedApplicationData.Current.LocalSettings.CreateContainer("Temporary", DistributedApplicationDataLocality.Temporary);

            DistributedApplicationData.Current.LocalSettings.DeleteContainer("Temporary");

            Assert.False(DistributedApplicationData.Current.LocalSettings.Containers.ContainsKey("Temporary"));
        }
    }
}