using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GPS.SettingsSync.Core;
using GPS.SettingsSync.Core.Collections;

namespace GPS.SettingsSyncTests
{
    public class Metadata : ISettingsMetadata
    {
        public string AppName { get; }
        public IDictionary<SettingsScopes, IDistributedPropertySet> BlankFile { get; }
            
        public Metadata([CallerMemberName]string appName = null
            , IDistributedPropertySet defaultLocalData = null
            , IDistributedPropertySet defaultRoamingData = null)
        {
            AppName = appName;

            var blankFiles = new ConcurrentDictionary<SettingsScopes, IDistributedPropertySet>();
            blankFiles.TryAdd(SettingsScopes.Local, defaultLocalData);
            blankFiles.TryAdd(SettingsScopes.Roaming, defaultRoamingData);

            BlankFile = blankFiles;
        }
    }
}