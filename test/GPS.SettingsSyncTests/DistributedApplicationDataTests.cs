using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GPS.SettingsSync.Core;
using Xunit;

namespace GPS.SettingsSyncTests
{
    public class DistributedApplicationDataTests
    {
        [Theory]
        [InlineData("string", "0", "int", int.MinValue)]
        public void ClearTest(string name1, object value1, string name2, object value2)
        {
            var container = DistributedApplicationData.Current.LocalSettings;

            Assert.NotNull(container);
            Assert.Equal(Assembly.GetEntryAssembly()?.GetName().Name, container.Name);
            Assert.Equal(DistributedApplicationDataLocality.Local, container.Locality);
            Assert.NotNull(container.Values);

            Assert.True(container.Values.TryAdd(name1, value1));
            Assert.True(container.Values.TryAdd(name2, value2));

            container = DistributedApplicationData.Current.LocalSettings.CreateContainer("Temporary", DistributedApplicationDataLocality.Temporary);

            Assert.Contains(DistributedApplicationData.Current.LocalSettings.Containers.Keys, s => s == "Temporary");
            Assert.NotNull(container);
            Assert.Equal("Temporary", container.Name);
            Assert.Equal(DistributedApplicationDataLocality.Temporary, container.Locality);
            Assert.NotNull(container.Values);
            Assert.Contains(DistributedApplicationData.Current.LocalSettings.Containers.Keys, s => s == "Temporary");

            Assert.True(container.Values.TryAdd(name1, value1));
            Assert.True(container.Values.TryAdd(name2, value2));

            DistributedApplicationData.Current.Clear();

            Assert.Empty(container.Values);
            Assert.Empty(DistributedApplicationData.Current.LocalSettings.Values);
        }

        [Fact()]
        public async Task SetVersionAsyncTest()
        {
            await DistributedApplicationData.Current.SetVersionAsync(1u, request =>
            {
                var pi = typeof(DistributedApplicationData).GetProperty("Version");
                pi.SetValue(DistributedApplicationData.Current, request.DesiredVersion);
            });

            Assert.Equal(1u, DistributedApplicationData.Current.Version);
        }

    }
}