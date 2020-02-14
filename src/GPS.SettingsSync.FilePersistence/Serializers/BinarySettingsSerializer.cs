using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using GPS.SettingsSync.Core.Collections;

namespace GPS.SettingsSync.FilePersistence.Serializers
{
    public class BinarySettingsSerializer : ISettingsSerializer
    {
        public byte[] Serialize(IDistributedPropertySet data)
        {
            using var stream = new MemoryStream();

            var formatter = new BinaryFormatter
            {
                AssemblyFormat = FormatterAssemblyStyle.Simple,
                FilterLevel = TypeFilterLevel.Full,
                TypeFormat = FormatterTypeStyle.TypesWhenNeeded
            };

            formatter.Serialize(stream, data);

            stream.Seek(0, SeekOrigin.Begin);

            return stream.GetBuffer();
        }

        public IDistributedPropertySet Deserialize(byte[] source)
        {
            try
            {
                using var stream = new MemoryStream(source);

                var formatter = new BinaryFormatter
                                {
                                    AssemblyFormat = FormatterAssemblyStyle.Simple,
                                    FilterLevel = TypeFilterLevel.Full,
                                    TypeFormat = FormatterTypeStyle.TypesWhenNeeded
                                };

                if (!(formatter.Deserialize(stream) is IDistributedPropertySet deserialized))
                {
                    throw new ApplicationException(
                        $"Source bytes note convertible to `IDistributedPropertySet`.");
                }

                return deserialized;
            }
            catch (SerializationException e) when (e.Message == "Attempting to deserialize an empty stream.") 
            {
                return new DistributedPropertySet();
            }
        }
    }
}