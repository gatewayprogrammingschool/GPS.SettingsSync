using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Serializers;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace GPS.SettingsSync.FilePersistence.Yaml.Serializers
{
    public class YamlSettingsSerializer : ISettingsSerializer
    {
        public class Pair
        {
            public string Key { get; set; }
            public object Value { get; set; }
        }

        public byte[] Serialize(IDistributedPropertySet data)
        {
            var targetData = new Dictionary<string, Pair>();
            data.ToList().ForEach(pair => targetData.Add(pair.Key, new Pair{ Key = pair.Value.GetType().FullName, Value = pair.Value}));

            var sb = new StringBuilder();
            using var textWriter = new StringWriter(sb);
            new Serializer().Serialize(textWriter, targetData);

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public IDistributedPropertySet Deserialize(byte[] source)
        {
            var yaml = Encoding.UTF8.GetString(source);
            var valueKindDictionary = new Deserializer().Deserialize<Dictionary<string, Pair>>(yaml);

            if (valueKindDictionary == null) return null;

            var results = new DistributedPropertySet();

            valueKindDictionary.ToList().ForEach(pair =>
            {
                try
                {
                    var toParse = pair.Value.Value.ToString();

                    results.TryAdd(pair.Key, Type.GetType(pair.Value.Key)?.Name switch
                    {
                        nameof(String) => toParse,
                        nameof(Int16) => short.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(UInt16) => ushort.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(Int32) => int.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(UInt32) => uint.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(Int64) => long.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(UInt64) => ulong.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(Decimal) => decimal.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(Single) => float.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(Double) => double.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(Boolean) => bool.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(Byte) => byte.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(SByte) => sbyte.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(Guid) => Guid.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(DateTime) => DateTime.TryParse(toParse, out var parsed) ? parsed : default,
                        nameof(DateTimeOffset) => ProcessDateTimeOffset(pair.Value.Value),
                        "Byte[]" => ProcessByteArray(pair.Value.Value),
                        _ => (object)toParse
                    });
                }
                catch (Exception e)
                {
                    e.Data[$"{nameof(pair)}.{nameof(pair.Key)}"] = pair.Key;
                    e.Data[$"{nameof(pair.Value)}.{nameof(pair.Value.Key)}"] = pair.Value.Key;
                    e.Data[$"{nameof(pair.Value)}.{nameof(pair.Value.Value)}"] = pair.Value.Value;

                    throw;
                }
            });

            return results;
        }

        private byte[] ProcessByteArray(object yamlValue)
        {
            if (yamlValue is List<object> values)
            {
                return values.Select(value => byte.TryParse(value?.ToString(), out var parsed) ? parsed : default).ToArray();
            }

            return null;
        }

        private DateTimeOffset ProcessDateTimeOffset(object yamlValue)
        {
            if (yamlValue is Dictionary<object, object> properties)
            {
                var dateTimeOffset = DateTimeOffset.TryParse(properties[nameof(DateTimeOffset.UtcDateTime)].ToString(), out var parsed) ? parsed : default;
                return dateTimeOffset;
            }

            return default(DateTimeOffset);
        }
    }
}