using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GPS.RandomDataGenerator;
using GPS.RandomDataGenerator.Extensions;
using GPS.RandomDataGenerator.Generators;
using GPS.RandomDataGenerator.Options;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence;
using GPS.SettingsSync.FilePersistence.Serializers;
using GPS.SettingsSync.FilePersistence.Tests;
using GPS.SettingsSync.FilePersistence.Yaml.Serializers;
using Microsoft.Extensions.DependencyInjection;

namespace GPS.SettingsSyncTests.Serializers
{
    public class SerializationData : IEnumerable<object[]>
    {
        private readonly Random _stringsRandom = new Random(0);
        private readonly Random _bytesRandom = new Random(0);
        private readonly Random _numbersRandom = new Random(0);
        private IDistributedPropertySet _set1;
        private IDistributedPropertySet _set2;
        private IDistributedPropertySet[] _sets;

        private IDistributedPropertySet Set1 => _set1 ??= new DistributedPropertySet().SetValues(new Dictionary<string, object>()
        {
            {"string", GetNextRandomString(GenerateStrings.FullName)},
            {"bool", true},
            {"byte[]", _bytesRandom.Generate<byte>(5, 0x0, 0xFF).ToArray()},
            {"decimal", GetNextRandomNumber<decimal>(0m, 255m)},
            {"int", GetNextRandomNumber<int>(0x0000_0000, 0x0000_00FF)},
            {"uint", GetNextRandomNumber<uint>(0x0000_0000u, 0x0000_00FFu)},
            {"double", GetNextRandomNumber<double>(0.0, 100.0)},
            {"DateTime", GetNextRandomNumber<DateTime>(DateTime.MinValue, DateTime.MaxValue)},
            {
                "DateTimeOffset",
                GetNextRandomNumber<DateTimeOffset>(DateTimeOffset.MinValue, DateTimeOffset.MaxValue)
            },
            {"Guid", GetNextRandomGuid()}
        });
        private IDistributedPropertySet Set2 => _set2 ??= new DistributedPropertySet().SetValues(new Dictionary<string, object>()
        {
            {"string", GetNextRandomString(GenerateStrings.FullName)},
            {"bool", false},
            {"byte[]", _bytesRandom.Generate<byte>(5, 0x0, 0xFF).ToArray()},
            {"decimal", GetNextRandomNumber<decimal>(0m, 255m)},
            {"int", GetNextRandomNumber<int>(0x0000_0000, 0x0000_00FF)},
            {"uint", GetNextRandomNumber<uint>(0x0000_0000u, 0x0000_00FFu)},
            {"double", GetNextRandomNumber<double>(0.0, 100.0)},
            {"DateTime", GetNextRandomNumber<DateTime>(DateTime.MinValue, DateTime.MaxValue)},
            {
                "DateTimeOffset",
                GetNextRandomNumber<DateTimeOffset>(DateTimeOffset.MinValue, DateTimeOffset.MaxValue)
            },
            {"Guid", GetNextRandomGuid()}
        });

        public IDistributedPropertySet[] Sets => 
            _sets ??= new [] {Set1, Set2};

        private IServiceProvider Provider { get; }

        public SerializationData()
        {
            TestStartup.Build();

            Provider = TestStartup.Provider;
        }

        public SerializationData(IServiceProvider provider)
        {
            Provider = provider;
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            var data = new List<object[]>
                       {
                           new object[] { Set1, new BinarySettingsSerializer() },
                           new object[] { Set1, new XmlSettingsSerializer() },
                           new object[] { Set1, new JsonSettingsSerializer() },
                           new object[] { Set1, new YamlSettingsSerializer() },
                           new object[] { Set2, new BinarySettingsSerializer() },
                           new object[] { Set2, new XmlSettingsSerializer() },
                           new object[] { Set2, new JsonSettingsSerializer() },
                           new object[] { Set2, new YamlSettingsSerializer() },
                       };

            foreach (var dataItem in data)
            {
                TestStartup.Build(fileType: dataItem.Last() switch
                {
                    BinarySettingsSerializer binary => FileTypes.Binary,
                    XmlSettingsSerializer xml => FileTypes.XML,
                    YamlSettingsSerializer yaml => FileTypes.Other,
                    _ => FileTypes.JSON
                });

                yield return dataItem;
            }
        }

        private string GetNextRandomString(GenerateStrings gs) => _stringsRandom.Next(Provider, gs);
        private TValue GetNextRandomNumber<TValue>(TValue min, TValue max) where TValue : IComparable => _numbersRandom.Next<TValue>(min, max);
        private Guid GetNextRandomGuid() => Provider.GetService<GuidGenerator>().Generate(0, 1).First();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}