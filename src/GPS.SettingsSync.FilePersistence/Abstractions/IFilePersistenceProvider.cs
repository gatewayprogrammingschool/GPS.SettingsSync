using System.Collections.Generic;
using GPS.SettingsSync.Core;
using GPS.SettingsSync.Core.Collections;

namespace GPS.SettingsSync.FilePersistence.Abstractions
{
    public interface IFilePersistenceProvider
    {
        DistributedApplicationDataContainer CreateFile(string name,
            string path,
            SettingsScopes settingsScope,
            IDistributedPropertySet seedValues = null);

        DistributedApplicationDataContainer OpenFile(string name, string path,
            SettingsScopes settingsScope
        );

        DistributedApplicationDataContainer WriteFile(string name,
            string path,
            SettingsScopes settingsScope,
            IDistributedPropertySet currentValues);

        bool DeleteFile(string name);

        FileTypes FileType { get; }
    }
}