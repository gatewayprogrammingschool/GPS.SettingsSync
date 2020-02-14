using System.Collections.Generic;
using GPS.SettingsSync.Core.Collections;

namespace GPS.SettingsSync.Core
{
    public interface ISettingsMetadata
    {
        string AppName { get; }
        IDictionary<SettingsScopes, IDistributedPropertySet> BlankFile { get; }
    }
}