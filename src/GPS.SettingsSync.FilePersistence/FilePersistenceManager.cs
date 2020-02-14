using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GPS.SettingsSync.Core;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Abstractions;
using GPS.SettingsSync.FilePersistence.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static GPS.SettingsSync.Core.Constants;

namespace GPS.SettingsSync.FilePersistence
{
    public class FilePersistenceManager
    {
        private static FilePersistenceManager _current;
        public static FilePersistenceManager Current => _current;

        public IConfiguration Configuration { get; }
        public ISettingsMetadata Metadata { get; }
        public IFilePersistenceProvider PersistenceProvider { get; }

        public string LocalPath { get; }
        public string RoamingPath { get; }

        public static FilePersistenceManager Build(IServiceProvider serviceProvider)
        {
            return _current = serviceProvider.GetService<FilePersistenceManager>();
        }

        public FilePersistenceManager(IServiceProvider provider
            , IConfiguration configuration
            , ISettingsMetadata metadata
            , IFilePersistenceProvider persistenceProvider)
        {
            _current = this;

            Configuration = configuration;
            Metadata = metadata;
            PersistenceProvider = persistenceProvider;

            LocalPath ??= configuration[SETTINGS_SYNC_DEFAULT_PATH_LOCAL] ??
                          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                              metadata.AppName);

            if (!File.Exists(Path.Combine(LocalPath, Metadata.AppName + $".{PersistenceProvider.FileExtension}")))
            {
                var fileWriter = PersistenceProvider.FileType switch
                {
                    FileTypes.Binary => (IFilePersistenceProvider) provider.GetService<BinaryPersistenceProvider>(),
                    FileTypes.XML => (IFilePersistenceProvider) provider.GetService<XmlPersistenceProvider>(),
                    FileTypes.Other => (IFilePersistenceProvider)provider.GetService(
                        Assembly
                            .Load(configuration[SETTINGS_SYNC_SETTINGS_FILE_OTHER_PROVIDER_ASSEMBLY])
                            .GetType(configuration[SETTINGS_SYNC_SETTINGS_FILE_OTHER_PROVIDER])),
                    _ => (IFilePersistenceProvider) provider.GetService<JsonPersistenceProvider>()
                };

                var blankFile = new DistributedPropertySet();
                blankFile.SetValues(Metadata.BlankFile[SettingsScopes.Local]);

                fileWriter.WriteFile(Metadata.AppName, LocalPath, SettingsScopes.Local, blankFile);
            }
            var localData = PersistenceProvider.OpenFile(Metadata.AppName, LocalPath, SettingsScopes.Local);

            AddApplicationData(localData, SettingsScopes.Local);

            RoamingPath ??= configuration[SETTINGS_SYNC_DEFAULT_PATH_ROAMING] ??
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                "One Drive", metadata.AppName);

            if (!File.Exists(Path.Combine(RoamingPath, Metadata.AppName + $".{PersistenceProvider.FileExtension}")))
            {
                var fileWriter = PersistenceProvider.FileType switch
                {
                    FileTypes.Binary => (IFilePersistenceProvider) provider.GetService<BinaryPersistenceProvider>(),
                    FileTypes.XML => (IFilePersistenceProvider) provider.GetService<XmlPersistenceProvider>(),
                    FileTypes.Other => (IFilePersistenceProvider)provider.GetService(
                        Assembly
                            .Load(configuration[SETTINGS_SYNC_SETTINGS_FILE_OTHER_PROVIDER_ASSEMBLY])
                            .GetType(configuration[SETTINGS_SYNC_SETTINGS_FILE_OTHER_PROVIDER])),
                    _ => (IFilePersistenceProvider) provider.GetService<JsonPersistenceProvider>()
                };

                var blankFile = new DistributedPropertySet();
                blankFile.SetValues(Metadata.BlankFile[SettingsScopes.Roaming]);

                fileWriter.WriteFile(Metadata.AppName, RoamingPath, SettingsScopes.Roaming, blankFile);
            }
            
            var roamingData = PersistenceProvider.OpenFile(Metadata.AppName, RoamingPath, SettingsScopes.Roaming);

            AddApplicationData(localData, SettingsScopes.Roaming);
        }

        public ConcurrentDictionary<(string Name, SettingsScopes Scope), FilePersistenceMapping> ApplicationData
        {
            get;
        } = new ConcurrentDictionary<(string Name, SettingsScopes Scope), FilePersistenceMapping>();

        public bool AddApplicationData(DistributedApplicationDataContainer applicationDataContainer, SettingsScopes settingsScopes)
        {
            var name = FilePersistenceManager.Current.Metadata.AppName;
            string path = null;

            if (ApplicationData.ContainsKey((name, settingsScopes))) return false;

            if(settingsScopes == SettingsScopes.Local)
            {
                path = FilePersistenceManager.Current.LocalPath;
                if (!File.Exists(Path.Combine(
                    path, $"{name}.{FilePersistenceManager.Current.PersistenceProvider.FileType}")))
                {
                    FilePersistenceManager.Current.PersistenceProvider.WriteFile(
                        name
                        , path
                        , SettingsScopes.Local
                        , applicationDataContainer.Values);
                }

                var localMapping = new FilePersistenceMapping(applicationDataContainer,
                    settingsScopes, name,
                    path);

                ApplicationData.TryAdd((name, SettingsScopes.Local), localMapping);
                return true;
            }

            if (settingsScopes != SettingsScopes.Roaming) return false;

            path = FilePersistenceManager.Current.RoamingPath;
            if (!File.Exists(Path.Combine(
                path, $"{name}.{FilePersistenceManager.Current.PersistenceProvider.FileType}")))
            {
                FilePersistenceManager.Current.PersistenceProvider.WriteFile(
                    name
                    , path
                    , SettingsScopes.Local
                    , applicationDataContainer.Values);
            }

            var roamingMapping = new FilePersistenceMapping(applicationDataContainer,
                settingsScopes, name,
                path);

            ApplicationData.TryAdd((name, SettingsScopes.Roaming), roamingMapping);

            return true;
        }

        public void ResetFile((string AppName, SettingsScopes Scope) scope)
        {
            var filePersistenceMapping = FilePersistenceManager.Current.ApplicationData[scope];
            filePersistenceMapping.Container.Values.EnableUpdates = false;
            filePersistenceMapping.Container.Values.Clear();
            filePersistenceMapping.Container.Values.EnableUpdates = true;
            filePersistenceMapping.Container.Values.SetValues(Metadata.BlankFile[filePersistenceMapping.SettingsScope]);
            
            PersistenceProvider.WriteFile(filePersistenceMapping.FileName, filePersistenceMapping.FilePath,
                filePersistenceMapping.SettingsScope, filePersistenceMapping.Container.Values);
        }

        public void UpdateFile(FilePersistenceMapping filePersistenceMapping)
        {
            PersistenceProvider.WriteFile(filePersistenceMapping.FileName, filePersistenceMapping.FilePath,
                filePersistenceMapping.SettingsScope, filePersistenceMapping.Container.Values);
        }
    }
}
