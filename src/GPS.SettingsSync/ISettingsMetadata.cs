using System.Collections.Generic;
using GPS.SettingsSync.Core.Collections;

namespace GPS.SettingsSync.Core
{
    public interface ISettingsMetadata
    {
        string AppName { get; }
        IReadOnlyDictionary<SettingsScopes, IDistributedPropertySet> BlankFile { get; }
    }
}