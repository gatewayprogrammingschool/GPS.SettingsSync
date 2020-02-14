using System;
using System.Collections.Generic;
using System.IO;
using GPS.SettingsSync.Core;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence;
using GPS.SettingsSync.FilePersistence.Abstractions;
using GPS.SettingsSync.FilePersistence.Serializers;
using GPS.SettingsSync.FilePersistence.Yaml.Serializers;
using GPS.SettingsSyncTests.Serializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace GPS.SettingsSyncTests
{
    public class FilePersistenceManagerTest
    {
        public ILogger<FilePersistenceManager> Logger { get; private set; }
        private TestStartup Startup { get; set; }

        public FilePersistenceManagerTest()
        {
            TestStartup.ConfigureServices -= TestStartupOnConfigureServices;
            TestStartup.ConfigureTests -= TestStartupOnConfigureTests;
            TestStartup.ConfigureServices += TestStartupOnConfigureServices;
            TestStartup.ConfigureTests += TestStartupOnConfigureTests;
        }

        private IConfigurationBuilder TestStartupOnConfigureTests(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("SettingsSync::DefaultPathLocal"
                    , Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(FilePersistenceManagerTest), "Local")),
                new KeyValuePair<string, string>("SettingsSync::DefaultPathRoaming"
                    , Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(FilePersistenceManagerTest), "Roaming")),
                new KeyValuePair<string, string>("SettingSync::UseFile", true.ToString()),
                new KeyValuePair<string, string>("SettingsSync::LocalPath"
                    , Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(FilePersistenceManagerTest), "Local")),
                new KeyValuePair<string, string>("SettingsSync::RoamingPath"
                    , Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(FilePersistenceManagerTest), "Roaming")),
            });

            return configurationBuilder;
        }

        private IServiceCollection TestStartupOnConfigureServices(IServiceCollection serviceCollection, IConfigurationRoot configuration)
        {
            return serviceCollection;
        }

        [Theory]
        [ClassData(typeof(SerializationData))]
        public void BuildTests(DistributedPropertySet dataSet, ISettingsSerializer serializer)
        {
            Startup = TestStartup.Build(fileType: serializer switch
            {
                BinarySettingsSerializer binary => FileTypes.Binary,
                XmlSettingsSerializer xml => FileTypes.XML,
                YamlSettingsSerializer yaml => FileTypes.Other,
                _ => FileTypes.JSON
            });
            Logger = TestStartup.Provider.GetService<ILogger<FilePersistenceManager>>();

            var manager = TestStartup.Provider.GetService<FilePersistenceManager>();
            
            Assert.NotNull(manager);
            Assert.NotNull(manager.ApplicationData);
            Assert.NotEmpty(manager.ApplicationData);
            Assert.NotNull(manager.Configuration);
            Assert.NotNull(manager.PersistenceProvider);
            Assert.NotNull(manager.LocalPath);
            Assert.NotNull(manager.RoamingPath);

            Logger.LogInformation($"Completed {nameof(FilePersistenceManagerTest)}.{nameof(BuildTests)}");
        }

        [Theory]
        [ClassData(typeof(SerializationData))]
        public void AddApplicationDataTest(DistributedPropertySet dataSet, ISettingsSerializer serializer)
        {
            Startup = TestStartup.Build(fileType: serializer switch
            {
                BinarySettingsSerializer binary => FileTypes.Binary,
                XmlSettingsSerializer xml => FileTypes.XML,
                YamlSettingsSerializer yaml => FileTypes.Other,
                _ => FileTypes.JSON
            });
            Logger = TestStartup.Provider.GetService<ILogger<FilePersistenceManager>>();

            var config = TestStartup.Configuration;

            var data = DistributedApplicationData.Current;
            data.AddFilePersistence(TestStartup.Provider
                , TestStartup.Provider.GetService<ISettingsMetadata>()
                , SettingsScopes.Both
                , config["SettingsSync::LocalPath"]
                , config["SettingsSync::RoamingPath"]);

            Assert.NotNull(data);
            Assert.NotNull(data.LocalSettings);
            Assert.NotNull(data.LocalSettings.Values);
            Assert.NotEmpty(data.LocalSettings.Values);
            Assert.NotNull(data.RoamingSettings);
            Assert.NotNull(data.RoamingSettings.Values);
            Assert.NotEmpty(data.RoamingSettings.Values);

            Logger.LogInformation($"Completed {nameof(FilePersistenceManagerTest)}.{nameof(AddApplicationDataTest)}");
        }

        [Theory]
        [ClassData(typeof(SerializationData))]
        public void ResetFileTest(DistributedPropertySet dataSet, ISettingsSerializer serializer)
        {
            Startup = TestStartup.Build(fileType: serializer switch
            {
                BinarySettingsSerializer binary => FileTypes.Binary,
                XmlSettingsSerializer xml => FileTypes.XML,
                YamlSettingsSerializer yaml => FileTypes.Other,
                _ => FileTypes.JSON
            });
            Logger = TestStartup.Provider.GetService<ILogger<FilePersistenceManager>>();

            var config = TestStartup.Configuration;

            var data = DistributedApplicationData.Current;
            data.AddFilePersistence(TestStartup.Provider
                , TestStartup.Provider.GetService<ISettingsMetadata>()
                , SettingsScopes.Both
                , config["SettingsSync::LocalPath"]
                , config["SettingsSync::RoamingPath"]);

            Assert.NotNull(data);
            Assert.NotNull(data.LocalSettings);
            Assert.NotNull(data.LocalSettings.Values);
            Assert.NotEmpty(data.LocalSettings.Values);
            Assert.NotNull(data.RoamingSettings);
            Assert.NotNull(data.RoamingSettings.Values);
            Assert.NotEmpty(data.RoamingSettings.Values);

            data.LocalSettings.Values.TryAdd("Test", "Test");

            var provider = TestStartup.Provider.GetService<IFilePersistenceProvider>();

            var fileData = provider.OpenFile(TestStartup.Provider.GetService<ISettingsMetadata>().AppName
                , config["SettingsSync::LocalPath"]
                , SettingsScopes.Local);

            Assert.NotNull(fileData);
            Assert.NotNull(fileData.Values);
            Assert.Contains("Test", fileData.Values.AsReadOnlyDictionary());

            var local = (FilePersistenceManager.Current.Metadata.AppName, SettingsScopes.Local);
            FilePersistenceManager.Current.ResetFile(local);

            Assert.False(data.LocalSettings.Values.TryGetValue("Test", out var _));

            Logger.LogInformation($"Completed {nameof(FilePersistenceManagerTest)}.{nameof(ResetFileTest)}");
        }

        [Theory]
        [ClassData(typeof(SerializationData))]
        public void UpdateFileTest(DistributedPropertySet dataSet, ISettingsSerializer serializer)
        {
            Startup = TestStartup.Build(fileType: serializer switch
            {
                BinarySettingsSerializer binary => FileTypes.Binary,
                XmlSettingsSerializer xml => FileTypes.XML,
                YamlSettingsSerializer yaml => FileTypes.Other,
                _ => FileTypes.JSON
            });
            Logger = TestStartup.Provider.GetService<ILogger<FilePersistenceManager>>();

            var config = TestStartup.Configuration;

            var data = DistributedApplicationData.Current;
            data.AddFilePersistence(TestStartup.Provider
                , TestStartup.Provider.GetService<ISettingsMetadata>()
                , SettingsScopes.Both
                , config[Constants.SETTINGS_SYNC_DEFAULT_PATH_LOCAL]
                , config[Constants.SETTINGS_SYNC_DEFAULT_PATH_ROAMING]);

            Assert.NotNull(data);
            Assert.NotNull(data.LocalSettings);
            Assert.NotNull(data.LocalSettings.Values);
            Assert.NotEmpty(data.LocalSettings.Values);
            Assert.NotNull(data.RoamingSettings);
            Assert.NotNull(data.RoamingSettings.Values);
            Assert.NotEmpty(data.RoamingSettings.Values);

            data.LocalSettings.Values.TryAdd("Test", "Test");

            var provider = TestStartup.Provider.GetService<IFilePersistenceProvider>();

            var fileData = provider.OpenFile(data.LocalSettings.Name
                , config[Constants.SETTINGS_SYNC_DEFAULT_PATH_LOCAL]
                , SettingsScopes.Local);

            Assert.NotNull(fileData);
            Assert.NotNull(fileData.Values);
            Assert.Contains("Test", fileData.Values.AsReadOnlyDictionary());

            Logger.LogInformation($"Completed {nameof(FilePersistenceManagerTest)}.{nameof(UpdateFileTest)}");
        }
    }
}