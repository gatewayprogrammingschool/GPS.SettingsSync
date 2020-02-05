using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GPS.RandomDataGenerator;
using GPS.RandomDataGenerator.Extensions;
using GPS.RandomDataGenerator.Generators;
using GPS.RandomDataGenerator.Options;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Serializers;
using GPS.SettingsSync.FilePersistence.Yaml.Serializers;
using Microsoft.Extensions.DependencyInjection;

namespace GPS.SettingsSyncTests.Serializers
{
    public class SerializationData : IEnumerable<object[]>
    {
        private readonly Random _stringsRandom = new Random(0);
        private readonly Random _bytesRandom = new Random(0);
        private readonly Random _numbersRandom = new Random(0);

        private IServiceProvider Provider { get; }

        public SerializationData()
        {
            var services = new ServiceCollection();
            services.AddGenerators();
            Provider = services.BuildServiceProvider(true);
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            var set1 = new DistributedPropertySet().SetValues(new Dictionary<string, object>()
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

            var data = new List<object[]>
            {
                new object[] { set1, new BinarySettingsSerializer() },
                new object[] { set1, new XmlSettingsSerializer() },
                new object[] { set1, new JsonSettingsSerializer() },
                new object[] { set1, new YamlSettingsSerializer() },
            };

            return data.GetEnumerator();
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