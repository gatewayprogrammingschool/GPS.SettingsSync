using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Abstractions;
using GPS.SettingsSync.FilePersistence.Serializers;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Providers
{
    public class JsonFileReader : IFileReader
    {
        public ILogger<JsonFileReader> Logger { get; }

        public JsonFileReader(ILogger<JsonFileReader> logger)
        {
            Logger = logger;
        }

        public IDistributedPropertySet ReadFile(string name, string path)
        {
            var filePath = Path.Combine(path, name + ".json");

            if(!File.Exists(filePath)) throw new FileNotFoundException(filePath);

            var serializer = new JsonSettingsSerializer();

            return serializer.Deserialize(File.ReadAllBytes(filePath));
        }
    }
}