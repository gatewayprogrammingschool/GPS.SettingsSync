using System.Collections.Generic;
using System.Text.Json.Serialization;
using GPS.SettingsSync.Core.Collections;

namespace GPS.SettingsSync.FilePersistence.Abstractions
{
    public interface IFileWriter
    {
        string WriteFile(string name, string path, IDistributedPropertySet data);
    }
}