using System.Collections.Generic;
using System.IO;
using GPS.SettingsSync.Core;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Abstractions;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Providers
{
    public abstract class PersistenceProviderBase : IFilePersistenceProvider
    {
        public ILogger Logger { get; }
        public IFileReader Reader { get; }
        public IFileWriter Writer { get; }
        public IFileRemover Remover { get; }
        public ISettingsMetadata Metadata { get; }

        public abstract FileTypes FileType { get; }

        protected PersistenceProviderBase(ILogger logger, IFileReader reader, IFileWriter writer,
            IFileRemover remover, ISettingsMetadata metadata)
        {
            Logger = logger;
            Reader = reader;
            Writer = writer;
            Remover = remover;
            Metadata = metadata;
        }

        public DistributedApplicationDataContainer CreateFile(string name, string path,
            SettingsScopes settingsScope,
            IDistributedPropertySet seedValues = null)
        {
            var filePath = Path.Combine(path, $"{name}.json");

            if (!File.Exists(filePath))
            {
                Writer.WriteFile(name, path, seedValues);
            }

            return OpenFile(name, path, settingsScope);
        }

        public DistributedApplicationDataContainer OpenFile(string name, string path, SettingsScopes settingsScope)
        {
            var filePath = Path.Combine(path, $"{name}.json");

            var data = Reader.ReadFile(name, path);

            return GetContainer(data, settingsScope, name);
        }

        private DistributedApplicationDataContainer GetContainer(
            IDistributedPropertySet data
            , SettingsScopes settingsScope
            , string name = "Temporary")
        {
            switch (settingsScope)
            {
                case SettingsScopes.Local:
                    DistributedApplicationData.Current.LocalSettings.Values.SetValues(data);
                    return DistributedApplicationData.Current.LocalSettings;

                case SettingsScopes.Roaming:
                    DistributedApplicationData.Current.RoamingSettings.Values.SetValues(data);
                    return DistributedApplicationData.Current.RoamingSettings;

                case SettingsScopes.Both:
                    return null;

                default:
                {
                    var container = DistributedApplicationData.Current.LocalSettings.CreateContainer(name,
                        DistributedApplicationDataLocality.Temporary);

                    container.Values.SetValues(data);
                    return container;
                }
            }
        }

        public DistributedApplicationDataContainer WriteFile(string name, string path, SettingsScopes settingsScope,
            IDistributedPropertySet currentValues)
        {
            throw new System.NotImplementedException();
        }

        public bool DeleteFile(string name)
        {
            throw new System.NotImplementedException();
        }
    }
}