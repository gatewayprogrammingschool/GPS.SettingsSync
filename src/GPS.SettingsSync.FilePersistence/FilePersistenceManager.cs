using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GPS.SettingsSync.Core;
using GPS.SettingsSync.FilePersistence.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            return serviceProvider.GetService<FilePersistenceManager>();
        }

        public FilePersistenceManager(IConfiguration configuration
            , ISettingsMetadata metadata
            , IFilePersistenceProvider persistenceProvider)
        {
            _current = this;

            Configuration = configuration;
            Metadata = metadata;
            PersistenceProvider = persistenceProvider;

            LocalPath = configuration["SettingsSync::DefaultPathLocal"] ??
                                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                    metadata.AppName);

            var localData = PersistenceProvider.OpenFile(Metadata.AppName + "Local", LocalPath, SettingsScopes.Local);

            AddApplicationData(localData, SettingsScopes.Local, metadata.AppName, LocalPath);

            RoamingPath = configuration["SettingsSystem::DefaultPathRoaming"] ??
                                  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                      "One Drive", metadata.AppName);

            var roamingData = PersistenceProvider.OpenFile(Metadata.AppName + "Roaming", LocalPath, SettingsScopes.Roaming);

            AddApplicationData(localData, SettingsScopes.Roaming, metadata.AppName, RoamingPath);
        }

        public ConcurrentDictionary<(string Name, SettingsScopes Scope), FilePersistenceMapping> ApplicationData
        {
            get;
        } = new ConcurrentDictionary<(string Name, SettingsScopes Scope), FilePersistenceMapping>();

        public bool AddApplicationData(DistributedApplicationDataContainer applicationData, SettingsScopes settingsScopes, string name, string path)
        {
            if (ApplicationData.ContainsKey((name, settingsScopes))) return false;

            if(settingsScopes == SettingsScopes.Local)
            {
                var localMapping = new FilePersistenceMapping(applicationData,
                    settingsScopes, name,
                    path);

                ApplicationData.TryAdd((name, SettingsScopes.Local), localMapping);
                return true;
            }

            if (settingsScopes != SettingsScopes.Roaming) return false;

            var roamingMapping = new FilePersistenceMapping(applicationData,
                settingsScopes, name,
                path);

            ApplicationData.TryAdd((name, SettingsScopes.Roaming), roamingMapping);

            return true;
        }

        public void ResetFile(FilePersistenceMapping filePersistenceMapping)
        {
            PersistenceProvider.WriteFile(filePersistenceMapping.FileName, filePersistenceMapping.FilePath,
                filePersistenceMapping.SettingsScope, Metadata.BlankFile[filePersistenceMapping.SettingsScope]);
        }

        public void UpdateFile(FilePersistenceMapping filePersistenceMapping)
        {
            PersistenceProvider.WriteFile(filePersistenceMapping.FileName, filePersistenceMapping.FilePath,
                filePersistenceMapping.SettingsScope, filePersistenceMapping.Container.Values);
        }
    }
}
