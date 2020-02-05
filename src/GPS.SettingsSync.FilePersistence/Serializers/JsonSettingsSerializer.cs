using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GPS.SettingsSync.Core.Collections;

namespace GPS.SettingsSync.FilePersistence.Serializers
{
    public class JsonSettingsSerializer : ISettingsSerializer
    {
        public byte[] Serialize(IDistributedPropertySet data)
        {
            var targetData = new Dictionary<string, KeyValuePair<string, object>>();
            data.ToList().ForEach(pair => targetData.Add(pair.Key, new KeyValuePair<string, object>(pair.Value.GetType().FullName, pair.Value)));
            return JsonSerializer.SerializeToUtf8Bytes(targetData, Options);
        }

        public IDistributedPropertySet Deserialize(byte[] source)
        {
            var valueKindDictionary = JsonSerializer.Deserialize<DistributedPropertySet>(source, Options);

            if (valueKindDictionary == null) return null;

            valueKindDictionary.ToList().ForEach(pair =>
            {
                
                if (!(pair.Value is JsonElement value)) return;

                var result = value.ValueKind switch
                {
                    JsonValueKind.Object => ProcessElement(value),
                    _ => new KeyValuePair<string, JsonElement>(value.ValueKind.ToString(), value)
                };

                try
                {
                    valueKindDictionary[pair.Key] = Type.GetType(result.Key)?.Name switch
                    {
                        nameof(String) => result.Value.ToString(),
                        nameof(Int16) => result.Value.GetInt16(),
                        nameof(UInt16) => result.Value.GetUInt16(),
                        nameof(Int32) => result.Value.GetInt32(),
                        nameof(UInt32) => result.Value.GetUInt32(),
                        nameof(Int64) => result.Value.GetInt64(),
                        nameof(UInt64) => result.Value.GetUInt64(),
                        nameof(Decimal) => result.Value.GetDecimal(),
                        nameof(Single) => result.Value.GetSingle(),
                        nameof(Double) => result.Value.GetDouble(),
                        nameof(Boolean) => result.Value.GetBoolean(),
                        nameof(Byte) => result.Value.GetByte(),
                        nameof(SByte) => result.Value.GetSByte(),
                        nameof(Guid) => result.Value.GetGuid(),
                        nameof(DateTime) => result.Value.GetDateTime(),
                        nameof(DateTimeOffset) => result.Value.GetDateTimeOffset(),
                        nameof(Array) => result.Value.EnumerateArray().ToArray(),
                        "Byte[]" => Convert.FromBase64String(result.Value.GetString()),
                        _ => (object)result.Value.ToString()
                    };
                }
                catch (Exception e)
                {
                    e.Data[$"{nameof(pair)}.{nameof(pair.Key)}"] = pair.Key;
                    e.Data[$"{nameof(pair)}.{nameof(pair.Value)}"] = pair.Value;
                    e.Data[$"{nameof(result)}.{nameof(result.Key)}"] = result.Key;
                    e.Data[$"{nameof(result)}.{nameof(result.Value)}"] = result.Value;

                    throw;
                }
            });

            return valueKindDictionary;

            KeyValuePair<string, JsonElement> ProcessElement(JsonElement element)
            {
                var innerJson = element.GetRawText();

                var innerElement = JsonSerializer.Deserialize<KeyValuePair<string, JsonElement>>(innerJson);

                return innerElement;
            }
        }

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            AllowTrailingCommas = false,
            WriteIndented = true,
            IgnoreNullValues = false,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = false,
            IgnoreReadOnlyProperties = false
        };
    }
}