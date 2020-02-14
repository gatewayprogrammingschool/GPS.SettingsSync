using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Abstractions;
using GPS.SettingsSync.FilePersistence.Serializers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Providers
{
    public class JsonFileReader : IFileReader
    {
        public IServiceProvider Provider { get; }
        public ILogger<JsonFileReader> Logger { get; }

        public JsonFileReader(IServiceProvider provider, ILogger<JsonFileReader> logger)
        {
            Provider = provider;
            Logger = logger;
        }

        public IDistributedPropertySet ReadFile(string name, string path)
        {
            var filePath = Path.Combine(path, name + ".json");

            if(!File.Exists(filePath)) throw new FileNotFoundException(filePath);

            var serializer = Provider.GetService<JsonSettingsSerializer>();

            return serializer.Deserialize(File.ReadAllBytes(filePath));
        }
    }
}