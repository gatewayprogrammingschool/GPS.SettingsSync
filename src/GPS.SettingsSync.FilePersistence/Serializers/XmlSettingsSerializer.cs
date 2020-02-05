using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using GPS.SettingsSync.Core.Collections;

namespace GPS.SettingsSync.FilePersistence.Serializers
{
    public class XmlSettingsSerializer : ISettingsSerializer
    {
        public byte[] Serialize(IDistributedPropertySet data)
        {
            using var memoryStream = new MemoryStream();
            using var xmlWriter = new XmlTextWriter(memoryStream, Encoding.Default)
            {
                Formatting = Formatting.Indented
            };

            data.WriteXml(xmlWriter);

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream.GetBuffer();
        }

        public IDistributedPropertySet Deserialize(byte[] source)
        {
            using var stream = new MemoryStream(source);
            using var xmlReader = new XmlTextReader(stream);

            var data = new DistributedPropertySet();
            data.ReadXml(xmlReader);

            return data;
        }
    }
}