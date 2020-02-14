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
    public class BinaryFileWriter : IFileWriter
    {
        public IServiceProvider Provider { get; }
        public ILogger<BinaryFileWriter> Logger { get; }

        public BinaryFileWriter(IServiceProvider provider, ILogger<BinaryFileWriter> logger)
        {
            Provider = provider;
            Logger = logger;
        }

        public string WriteFile(string name, string path, IDistributedPropertySet data)
        {
            var filePath = Path.Combine(path, name + $".{FileTypes.Binary}");

            var serializer = Provider.GetService<BinarySettingsSerializer>();

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
                Logger.LogCritical(e, "Exception caught writing Binary file.");
                throw;
            }
        }
    }
}