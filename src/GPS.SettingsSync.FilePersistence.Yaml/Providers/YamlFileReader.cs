using System;
using System.Collections.Generic;
using System.IO;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Abstractions;
using GPS.SettingsSync.FilePersistence.Yaml.Serializers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Yaml.Providers
{
    public class YamlFileReader : IFileReader
    {
        public IServiceProvider Provider { get; }
        public ILogger<YamlFileReader> Logger { get; }

        public YamlFileReader(IServiceProvider provider, ILogger<YamlFileReader> logger)
        {
            Provider = provider;
            Logger = logger;
        }


        public IDistributedPropertySet ReadFile(string name, string path)
        {
            var filePath = Path.Combine(path, name + ".yaml");

            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

            var serializer = Provider.GetService<YamlSettingsSerializer>();
                
            var data = serializer.Deserialize(File.ReadAllBytes(filePath));

            return data;
        }
    }
}