using System;
using System.Reflection;
using GPS.SettingsSync.Core;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Abstractions;
using GPS.SettingsSync.FilePersistence.Providers;
using GPS.SettingsSync.FilePersistence.Serializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static GPS.SettingsSync.Core.Constants;

namespace GPS.SettingsSync.FilePersistence
{
    public static class FilePersistenceManagerExtensions
    {
        public static DistributedApplicationData AddFilePersistence(
            this DistributedApplicationData applicationData
            , IServiceProvider provider
            , ISettingsMetadata metadata
            , SettingsScopes settingsScope = SettingsScopes.Both
            , string localRootPath = ".\\Local"
            , string roamingRootPath = ".\\Roaming")
        {
            FilePersistenceManager.Build(provider);

            if(settingsScope == SettingsScopes.Both || settingsScope == SettingsScopes.Local)
            {
                FilePersistenceManager.Current.AddApplicationData(
                    applicationData.LocalSettings, settingsScope);
            }

            if (settingsScope == SettingsScopes.Both || settingsScope == SettingsScopes.Roaming)
            {
                FilePersistenceManager.Current.AddApplicationData(
                    applicationData.RoamingSettings, settingsScope);
            }

            applicationData.DataChanged += ApplicationDataOnDataChanged;

            return applicationData;
        }

        private static void ApplicationDataOnDataChanged(DistributedApplicationData sender, DistributedApplicationDataContainer container)
        {
            //var local = (FilePersistenceManager.Current.Metadata.AppName, SettingsScopes.Local);
            //FilePersistenceManager.Current.UpdateFile(FilePersistenceManager.Current.ApplicationData[local]);

            //var roaming = (FilePersistenceManager.Current.Metadata.AppName, SettingsScopes.Roaming);
            //FilePersistenceManager.Current.UpdateFile(FilePersistenceManager.Current.ApplicationData[roaming]);
        }


        public static IServiceCollection AddDistributedApplicationData(
            this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<DistributedApplicationData>(provider =>
                {
                    var config = provider.GetService<IConfiguration>();

                    if (bool.TryParse(config["SettingSync::UseFile"], out var useFile) && useFile)
                    {
                        DistributedApplicationData.Current.AddFilePersistence(
                            provider
                            , provider.GetService<ISettingsMetadata>()
                            , SettingsScopes.Both
                            , config["SettingsSync::LocalPath"]
                            , config["SettingsSync::RoamingPath"]);
                    }

                    return DistributedApplicationData.Current;
                });

            return serviceCollection;
        }

        public static IServiceCollection AddFilePersistenceManager(
            this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<JsonFileReader, JsonFileReader>()
                .AddSingleton<JsonFileWriter, JsonFileWriter>()
                .AddSingleton<XmlFileReader, XmlFileReader>()
                .AddSingleton<XmlFileWriter, XmlFileWriter>()
                .AddSingleton<JsonSettingsSerializer, JsonSettingsSerializer>()
                .AddSingleton<XmlSettingsSerializer, XmlSettingsSerializer>()
                .AddSingleton<BinarySettingsSerializer, BinarySettingsSerializer>()
                .AddSingleton<IFileRemover, FileRemover>()
                .AddSingleton<BinaryFileReader, BinaryFileReader>()
                .AddSingleton<BinaryFileWriter, BinaryFileWriter>()
                .AddSingleton<BinaryPersistenceProvider, BinaryPersistenceProvider>()
                .AddSingleton<JsonPersistenceProvider, JsonPersistenceProvider>()
                .AddSingleton<XmlPersistenceProvider, XmlPersistenceProvider>()
                .AddSingleton<IFilePersistenceProvider>(provider =>
                {
                    var configuration = provider.GetService<IConfiguration>();
                    var fileProvider =
                        (Enum.TryParse(configuration[$"SettingsSync::SettingsFileType"], out FileTypes fileType)
                                ? fileType
                                : FileTypes.Other) switch
                            {
                                FileTypes.Binary => provider.GetService<BinaryPersistenceProvider>(),
                                FileTypes.XML => provider.GetService<XmlPersistenceProvider>(),
                                FileTypes.JSON => provider.GetService<JsonPersistenceProvider>(),
                                _ => GetProvider()
                            };

                    return fileProvider;

                    IFilePersistenceProvider GetProvider()
                    {
                        var assemblyName = configuration[SETTINGS_SYNC_SETTINGS_FILE_OTHER_PROVIDER_ASSEMBLY];
                        var typeName = configuration[SETTINGS_SYNC_SETTINGS_FILE_OTHER_PROVIDER];
                        var assembly = Assembly.Load(assemblyName);
                        var type = assembly.GetType(typeName, true, true);

                        return (IFilePersistenceProvider)ActivatorUtilities.CreateInstance(provider, type);
                    }
                })
                .AddSingleton<FilePersistenceManager, FilePersistenceManager>();

            return serviceCollection;
        }

        private static PersistenceProviderBase GetOtherPersistenceProvider(string fileTypeName)
        {
            var fileType = Type.ReflectionOnlyGetType(fileTypeName, false, true);

            if(fileType == null) throw new ArgumentException($"Unknown type: `{fileTypeName}`", nameof(fileTypeName));

            if(!(Activator.CreateInstance(fileType) is PersistenceProviderBase provider)) 
                throw new ArgumentException($"Type `{fileTypeName}` cannot be cast to `PersistenceProviderBase`.");

            return provider;
        }
    }
}