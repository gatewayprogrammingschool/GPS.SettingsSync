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
    public class JsonFileWriter : IFileWriter
    {
        public IServiceProvider Provider { get; }
        public ILogger<JsonFileWriter> Logger { get; }

        public JsonFileWriter(IServiceProvider provider, ILogger<JsonFileWriter> logger)
        {
            Provider = provider;
            Logger = logger;
        }

        public string WriteFile(string name, string path, IDistributedPropertySet data)
        {
            var filePath = Path.Combine(path, name + ".json");

            var serializer = Provider.GetService<JsonSettingsSerializer>();

            var bytes = serializer.Serialize(data);

            Logger.LogInformation($"Serialized Length: {bytes.Length}");

            try
            {
                System.IO.File.WriteAllBytes(filePath, bytes);

                Logger.LogDebug($"Write {bytes.Length} bytes to {filePath}.");

                return filePath;
            }
            catch (Exception e)
            {
                Logger.LogCritical(e, "Exception caught writing JSON file.");
                throw;
            }
        }
    }
}