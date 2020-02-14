using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Abstractions;
using GPS.SettingsSync.FilePersistence.Yaml.Serializers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Yaml.Providers
{
    public class YamlFileWriter : IFileWriter
    {
        public IServiceProvider Provider { get; }
        public ILogger<YamlFileWriter> Logger { get; }

        public YamlFileWriter(IServiceProvider provider, ILogger<YamlFileWriter> logger)
        {
            Provider = provider;
            Logger = logger;
        }

        public string WriteFile(string name, string path, IDistributedPropertySet data)
        {
            var filePath = Path.Combine(path, name + ".yaml");

            var serializer = Provider.GetService<YamlSettingsSerializer>();

            try
            {
                var yaml = serializer.Serialize(data);

                File.WriteAllBytes(filePath, yaml);

                Logger.LogDebug($"Wrote {yaml.Length} bytes to {filePath}.");
                Logger.LogDebug(Encoding.UTF8.GetString(yaml));

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