using System;
using System.Collections.Generic;
using GPS.RandomDataGenerator;
using GPS.SettingsSync.Core;
using GPS.SettingsSync.FilePersistence;
using GPS.SettingsSync.FilePersistence.Yaml;
using GPS.SettingsSync.FilePersistence.Yaml.Providers;
using GPS.SettingsSyncTests.Serializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static GPS.SettingsSync.Core.Constants;

namespace GPS.SettingsSyncTests
{
    public class TestStartup
    {
        public static IServiceProvider Provider { get; private set; }
        public static IConfigurationRoot Configuration { get; private set; }

        public ILogger<TestStartup> Logger { get; }

        public static event Func<IServiceCollection, IConfigurationRoot, IServiceCollection> ConfigureServices;

        public static event Func<IConfigurationBuilder, IConfigurationBuilder> ConfigureTests;

        public static TestStartup Build(FileTypes fileType = FileTypes.JSON)
        {
            var serviceCollection = new ServiceCollection();

            var configBuilder = new ConfigurationBuilder();

            configBuilder.AddInMemoryCollection(
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(LOGGING_CONSOLE_LOGLEVEL, LogLevel.Debug.ToString()),
                    new KeyValuePair<string, string>(LOGGING_TRACEWRITER_LOGLEVEL, LogLevel.Trace.ToString()),
                    new KeyValuePair<string, string>(SETTINGS_SYNC_SETTINGS_FILE_TYPE, fileType.ToString()),
                    new KeyValuePair<string, string>(SETTINGS_SYNC_SETTINGS_FILE_OTHER_PROVIDER,
                        typeof(YamlPersistenceProvider).FullName),
                    new KeyValuePair<string, string>(SETTINGS_SYNC_SETTINGS_FILE_OTHER_PROVIDER_ASSEMBLY,
                        "GPS.SettingsSync.FilePersistence.Yaml"),
                    new KeyValuePair<string, string>(SETTINGS_SYNC_SETTINGS_FILE_EXTENSION, "yaml")
                });

            ConfigureTests?.Invoke(configBuilder);

            Configuration = configBuilder.Build();

            ConfigureServices?.Invoke(serviceCollection, Configuration);

            serviceCollection
                .AddSingleton<IConfiguration>(provider => Configuration)
                .AddSingleton<ISettingsMetadata>(provider =>
                {
                    var data = provider.GetService<SerializationData>();
                    var localData = data.Sets[0];
                    var roamingData = data.Sets[1];

                    return new Metadata("testhost", defaultLocalData: localData, defaultRoamingData: roamingData);
                })
                .AddTransient<TestStartup, TestStartup>()
                .AddTransient<SerializationData, SerializationData>()
                .AddGenerators()
                .AddDistributedApplicationData()
                .AddFilePersistenceManager()
                .AddYamlProvider()
                .AddLogging(builder =>
                {
                    builder
                        .AddConfiguration(Configuration)
                        .AddDebug();
                });

            Provider = serviceCollection.BuildServiceProvider(true);

            return Provider.GetService<TestStartup>();
        }

        public TestStartup(
            ILogger<TestStartup> logger)
        {
            Logger = logger;
        }
    }
}