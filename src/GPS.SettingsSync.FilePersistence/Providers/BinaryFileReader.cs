using System;
using System.Collections.Generic;
using System.IO;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Abstractions;
using GPS.SettingsSync.FilePersistence.Serializers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Providers
{
    public class BinaryFileReader : IFileReader
    {
        public IServiceProvider Provider { get; }
        public ILogger<BinaryFileReader> Logger { get; }

        public BinaryFileReader(IServiceProvider provider, ILogger<BinaryFileReader> logger)
        {
            Provider = provider;
            Logger = logger;
        }

        public IDistributedPropertySet ReadFile(string name, string path)
        {
            var filePath = Path.Combine(path, name + $".{FileTypes.Binary}");

            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

            var serializer = Provider.GetService<BinarySettingsSerializer>();

            return serializer.Deserialize(File.ReadAllBytes(filePath));
        }
    }
}