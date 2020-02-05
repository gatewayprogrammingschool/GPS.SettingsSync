using System;
using GPS.SettingsSync.Core;
using GPS.SettingsSync.FilePersistence.Abstractions;
using GPS.SettingsSync.FilePersistence.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence
{
    public static class FilePersistenceManagerExtensions
    {
        public static DistributedApplicationData AddFilePersistence(
            this DistributedApplicationData applicationData
            , ISettingsMetadata metadata
            , SettingsScopes settingsScope = SettingsScopes.Both
            , string localRootPath = ".\\Local"
            , string roamingRootPath = ".\\Roaming")
        {
            if(settingsScope == SettingsScopes.Both || settingsScope == SettingsScopes.Local)
            {
                FilePersistenceManager.Current.AddApplicationData(applicationData.LocalSettings, 
                    settingsScope, metadata.AppName, localRootPath);
            }

            if (settingsScope == SettingsScopes.Both || settingsScope == SettingsScopes.Roaming)
            {
                FilePersistenceManager.Current.AddApplicationData(applicationData.RoamingSettings, 
                    settingsScope, metadata.AppName, roamingRootPath);
            }

            return applicationData;
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
                            provider.GetService<ISettingsMetadata>()
                            , SettingsScopes.Both
                            , config["SettingsSync::LocalPath"]
                            , config["SettingsSync::RoamingPath"]);
                    }

                    return DistributedApplicationData.Current;
                });

            return serviceCollection;
        }

        public static IServiceCollection AddFilePersistenceManager(
            this IServiceCollection serviceCollection
            , Func<IServiceProvider, ISettingsMetadata> metadata)
        {
            serviceCollection
                .AddSingleton<ISettingsMetadata>(metadata)
                .AddTransient<IFileReader>(provider =>
            {
                var config = provider.GetService<IConfiguration>();

                var fileTypeName = config["SettingsSync::FileType"];

                Enum.TryParse(fileTypeName, out FileTypes fileType);

                return fileType switch
                {
                    FileTypes.Binary => new BinaryFileReader(provider.GetService<ILogger<BinaryFileReader>>()) as IFileReader,
                    FileTypes.XML => new XmlFileReader(provider.GetService<ILogger<XmlFileReader>>()) as IFileReader,
                    _ => new JsonFileReader(provider.GetService<ILogger<JsonFileReader>>()) as IFileReader
                };
            })
                .AddTransient<IFileWriter>(provider =>
            {
                var config = provider.GetService<IConfiguration>();

                var fileTypeName = config["SettingsSync::FileType"];

                Enum.TryParse(fileTypeName, out FileTypes fileType);

                return fileType switch
                {
                    FileTypes.Binary => new BinaryFileWriter(provider.GetService<ILogger<BinaryFileWriter>>()) as IFileWriter,
                    FileTypes.XML => new XmlFileWriter(provider.GetService<ILogger<XmlFileWriter>>()) as IFileWriter,
                    _ => new JsonFileWriter(provider.GetService<ILogger<JsonFileWriter>>()) as IFileWriter
                };
            })
                .AddTransient<IFilePersistenceProvider>(provider =>
                {
                    var config = provider.GetService<IConfiguration>();

                    var fileTypeName = config["SettingsSync::FileType"];

                    Enum.TryParse(fileTypeName, out FileTypes fileType);

                    var persistenceProvider = fileType switch
                    {
                        FileTypes.Other => GetOtherPersistenceProvider(fileTypeName) as PersistenceProviderBase,
                        FileTypes.Binary => new BinaryPersistenceProvider(
                            provider.GetService<IFileReader>() as BinaryFileReader
                            , provider.GetService<IFileWriter>() as BinaryFileWriter
                            , provider.GetService<IFileRemover>()
                            , provider.GetService<ILogger<BinaryPersistenceProvider>>()
                            , metadata(provider)) as PersistenceProviderBase,
                        FileTypes.XML => new XmlPersistenceProvider(
                            provider.GetService<IFileReader>() as XmlFileReader
                            , provider.GetService<IFileWriter>() as XmlFileWriter
                            , provider.GetService<IFileRemover>()
                            , provider.GetService<ILogger<XmlPersistenceProvider>>()
                            , metadata(provider)) as PersistenceProviderBase,
                        _ => new JsonPersistenceProvider(
                            provider.GetService<IFileReader>() as JsonFileReader
                            , provider.GetService<IFileWriter>() as JsonFileWriter
                            , provider.GetService<IFileRemover>()
                            , provider.GetService<ILogger<JsonPersistenceProvider>>()
                            , metadata(provider)) as PersistenceProviderBase
                    };

                    return persistenceProvider;
                })
                .AddSingleton<FilePersistenceManager>(provider =>
            {
                var config = provider.GetService<IConfiguration>();
                var manager = new FilePersistenceManager(config, metadata(provider), provider.GetService<IFilePersistenceProvider>());

                return manager;
            });

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