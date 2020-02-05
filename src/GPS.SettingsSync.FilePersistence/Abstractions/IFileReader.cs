using System.Collections.Generic;
using System.Net;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Providers;

namespace GPS.SettingsSync.FilePersistence.Abstractions
{
    public interface IFileReader
    {
        IDistributedPropertySet ReadFile(string name, string path);
    }
}