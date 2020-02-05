using System.Collections.Generic;
using System.IO;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Abstractions;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Yaml.Providers
{
    public class YamlFileReader : IFileReader
    {
        public ILogger<YamlFileReader> Logger { get; }

        public YamlFileReader(ILogger<YamlFileReader> logger)
        {
            Logger = logger;
        }


        public IDistributedPropertySet ReadFile(string name, string path)
        {
            var filePath = Path.Combine(path, name + ".yaml");

            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

            var data = new YamlDotNet.Serialization.Deserializer().Deserialize<IDistributedPropertySet>(filePath);

            return data;
        }
    }
}