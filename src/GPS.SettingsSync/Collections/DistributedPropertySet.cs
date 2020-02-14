using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GPS.SettingsSync.Core.Collections
{
    [Serializable]
    public sealed class DistributedPropertySet : ConcurrentDictionary<string, object>
        , IDistributedPropertySet
    {
        public DistributedPropertySet() { }

        public DistributedPropertySet(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException(nameof(info));

            var keys = info.GetString(nameof(Keys)).Split('|');
            var types = info.GetString("Types").Split('|');
            var index = 0;

            foreach (var key in keys)
            {
                TryAdd(key, info.GetValue(key, Type.GetType(types[index++]) ?? typeof(object)));
            }
        }

        public bool EnableUpdates { get; set; } = true;
        
        public new bool TryAdd(string key, object value)
        {
            base.AddOrUpdate(key, value, (s, o) => value);

            if(EnableUpdates) MapChanged?.Invoke(this, new MapChangedEventArgs<string>(key, DistributedCollectionChange.ItemInserted));

            return true;
        }

        public new object AddOrUpdate(string key, object addValue, Func<string, object, object> updateFactory)
        {
            if (ContainsKey(key))
            {
                TryUpdate(key, updateFactory.Invoke(key, this[key]), this[key]);
            }
            else
            {
                TryAdd(key, addValue);
            }

            return TryGetValue(key, out var value) ? value : default;
        }

        public new object AddOrUpdate(string key, Func<string, object> addFactory, Func<string, object, object> updateFactory)
        {
            if (ContainsKey(key))
            {
                TryUpdate(key, updateFactory.Invoke(key, this[key]), this[key]);
            }
            else
            {
                TryAdd(key, addFactory.Invoke(key));
            }

            return TryGetValue(key, out object value) ? value : default;
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            if (base.TryGetValue(key, out var innerValue))
            {
                value = (TValue)innerValue;
                return true;
            }

            throw new InvalidCastException($"Cannot cast from {innerValue.GetType().Name} to {typeof(TValue).Name}");
        }

        public new object GetOrAdd(string key, object value)
        {
            if (ContainsKey(key))
            {
                return this[key];
            }

            return TryAdd(key, value) ? value : default;
        }

        public new object GetOrAdd(string key, Func<string, object> valueFactory)
        {
            if (ContainsKey(key))
            {
                return this[key];
            }

            return TryAdd(key, valueFactory.Invoke(key)) ? this[key] : default;
        }

        public object GetOrAdd<TArg>(string key, Func<string, TArg, object> valueFactory, TArg arg)
        {
            if (ContainsKey(key))
            {
                return this[key];
            }

            return TryAdd(key, valueFactory.Invoke(key, arg)) ? this[key] : default;
        }

        public new bool TryRemove(string key, out object value)
        {
            if (!base.TryRemove(key, out var removed))
            {
                value = default;
                return false;
            }

            if(EnableUpdates) MapChanged?.Invoke(this, new MapChangedEventArgs<string>(key, DistributedCollectionChange.ItemRemoved));

            value = removed;

            return true;
        }

        public new bool TryUpdate(string key, object newValue, object comparisonValue)
        {
            if (!base.TryUpdate(key, newValue, comparisonValue)) return false;

            if(EnableUpdates) MapChanged?.Invoke(this, new MapChangedEventArgs<string>(key, DistributedCollectionChange.ItemChanged));

            return true;
        }

        public new void Clear()
        {
            base.Clear();

            if(EnableUpdates) MapChanged?.Invoke(this, new MapChangedEventArgs<string>((string)null, DistributedCollectionChange.Reset));
        }

        public event MapChangedEventHandler<string, object> MapChanged;
        public IReadOnlyDictionary<string, object> AsReadOnlyDictionary()
        {
            return (IReadOnlyDictionary<string, object>) this.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public IDistributedPropertySet SetValues(IDictionary<string, object> data)
        {
            foreach (var keyValuePair in data)
            {
                AddOrUpdate(keyValuePair.Key, keyValuePair.Value, (s, o) => keyValuePair.Value);
            }

            return this;
        }

        [SecurityPermission(SecurityAction.LinkDemand,
            Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException(nameof(info));

            info.AddValue(nameof(Keys), string.Join("|", Keys));
            info.AddValue("Types", string.Join("|", Values.Select(value => value.GetType().FullName)));

            this.ToList().ForEach(pair =>
            {
                info.AddValue(pair.Key, pair.Value);
            });
        }

        public XmlSchema GetSchema()
        {
            return new XmlSchema();
        }

        public void ReadXml(XmlReader reader)
        {
            Clear();

            var doc = new XmlDocument();

            try
            {
                doc.Load(reader);

                var pairs = doc.DocumentElement.SelectNodes(
                    $"//{nameof(DistributedPropertySet)}/{nameof(Values)}").Cast<XmlElement>();

                foreach (var pair in pairs)
                {
                    var key  = pair.Attributes[nameof(KeyValuePair<string, object>.Key)].Value;
                    var type = pair.Attributes[nameof(Type)].Value;
                    var value = type switch
                    {
                        "System.Boolean" => bool.TryParse(pair.InnerText, out var boolValue) ? boolValue : default,
                        "System.Byte" => byte.TryParse(pair.InnerText, out var byteValue) ? byteValue : default,
                        "System.SByte" => sbyte.TryParse(pair.InnerText, out var sbyteValue) ? sbyteValue : default,
                        "System.Int16" => short.TryParse(pair.InnerText, out var shortValue) ? shortValue : default,
                        "System.UInt16" => ushort.TryParse(pair.InnerText, out var ushortValue) ? ushortValue : default,
                        "System.Int32" => int.TryParse(pair.InnerText, out var intValue) ? intValue : default,
                        "System.UInt32" => uint.TryParse(pair.InnerText, out var uintValue) ? uintValue : default,
                        "System.Int64" => long.TryParse(pair.InnerText, out var longValue) ? longValue : default,
                        "System.UInt64" => ulong.TryParse(pair.InnerText, out var ulongValue) ? ulongValue : default,
                        "System.Single" => float.TryParse(pair.InnerText, out var floatValue) ? floatValue : default,
                        "System.Double" => double.TryParse(pair.InnerText, out var doubleValue) ? doubleValue : default,
                        "System.Decimal" => decimal.TryParse(pair.InnerText, out var decimalValue)
                            ? decimalValue
                            : default,
                        "System.DateTime" => DateTime.TryParse(pair.InnerText, out var dateTimeValue)
                            ? dateTimeValue
                            : default,
                        "System.DateTimeOffset" => DateTimeOffset.TryParse(pair.InnerText, out var dateTimeOffsetValue)
                            ? dateTimeOffsetValue
                            : default,
                        "System.String" => pair.InnerText,
                        "System.Byte[]" => Convert.FromBase64String(pair.InnerText),
                        "System.Guid" => Guid.TryParse(pair.InnerText, out var guidValue) ? guidValue : default,
                        _ => (object) pair.InnerText
                    };

                    TryAdd(key, value);
                }
            }
            catch (XmlException e) when (e.Message == "Root element is missing.")
            {
                // Do nothing.
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            var doc = new XmlDocument();

            doc.AppendChild(doc.CreateElement(nameof(DistributedPropertySet)));

            this.ToList().ForEach(pair =>
            {
                var element = doc.CreateElement(nameof(Values));

                var key = doc.CreateAttribute(nameof(KeyValuePair<string, object>.Key));
                key.Value = pair.Key;
                element.Attributes.Append(key);

                var type = doc.CreateAttribute(nameof(Type));
                type.Value = pair.Value.GetType().FullName;
                element.Attributes.Append(type);

                if (pair.Value is byte[] bytes)
                {
                    element.InnerText = Convert.ToBase64String(bytes);
                }
                else
                {
                    element.InnerText = pair.Value.ToString();
                }

                doc.DocumentElement.AppendChild(element);
            });

            doc.Save(writer);
        }
    }
}