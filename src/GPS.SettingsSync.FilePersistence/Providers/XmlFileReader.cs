using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Abstractions;
using GPS.SettingsSync.FilePersistence.Serializers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Providers
{
    public class XmlFileReader : IFileReader
    {
        public IServiceProvider Provider { get; }
        public ILogger<XmlFileReader> Logger { get; }

        public XmlFileReader(IServiceProvider provider, ILogger<XmlFileReader> logger)
        {
            Provider = provider;
            Logger = logger;
        }


        public IDistributedPropertySet ReadFile(string name, string path)
        {
            var filePath = Path.Combine(path, name + ".xml");

            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

            var serializer = Provider.GetService<XmlSettingsSerializer>();

            return serializer.Deserialize(File.ReadAllBytes(filePath));
        }
    }
}