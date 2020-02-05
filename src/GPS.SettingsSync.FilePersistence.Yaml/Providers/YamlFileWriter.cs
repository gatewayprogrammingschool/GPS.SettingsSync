using System;
using System.Collections.Generic;
using System.IO;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Abstractions;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Yaml.Providers
{
    public class YamlFileWriter : IFileWriter
    {
        public ILogger<YamlFileWriter> Logger { get; }

        public YamlFileWriter(ILogger<YamlFileWriter> logger)
        {
            Logger = logger;
        }

        public string WriteFile(string name, string path, IDistributedPropertySet data)
        {
            var filePath = Path.Combine(path, name + ".yaml");

            var serializer = new YamlDotNet.Serialization.Serializer();

            try
            {
                var yaml = serializer.Serialize(data);

                File.WriteAllText(filePath, yaml);

                Logger.LogDebug($"Wrote {yaml.Length} bytes to {filePath}.");
                Logger.LogDebug(yaml);

                return filePath;
            }
            catch (Exception e)
            {
                Logger.LogCritical(e, "Exception serializing to Yaml.");
                throw;
            }
        }
    }
}