using System.Collections.Generic;
using System.IO;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Abstractions;
using GPS.SettingsSync.FilePersistence.Serializers;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Providers
{
    public class BinaryFileReader : IFileReader
    {
        public ILogger<BinaryFileReader> Logger { get; }

        public BinaryFileReader(ILogger<BinaryFileReader> logger)
        {
            Logger = logger;
        }

        public IDistributedPropertySet ReadFile(string name, string path)
        {
            var filePath = Path.Combine(path, name + ".bin");

            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

            var serializer = new BinarySettingsSerializer();

            return serializer.Deserialize(File.ReadAllBytes(filePath));
        }
    }
}