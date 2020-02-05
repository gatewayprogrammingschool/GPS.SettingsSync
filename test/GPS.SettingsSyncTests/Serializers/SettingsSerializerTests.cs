using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GPS.SettingsSync.Core.Collections;
using GPS.SettingsSync.FilePersistence.Serializers;
using Xunit;
using Xunit.Abstractions;

namespace GPS.SettingsSyncTests.Serializers
{
    public class SettingsSerializerTests
    {
        private readonly ITestOutputHelper _output;

        public SettingsSerializerTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Theory]
        [ClassData(typeof(SerializationData))]
        public void SerializeTest(IDistributedPropertySet data, ISettingsSerializer serializer)
        {
            byte[] result = null;

            try
            {
                result = serializer.Serialize(data);

                _output.WriteLine(Encoding.UTF8.GetString(result));
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                e.Data.Keys.Cast<string>().ToList().ForEach(key => _output.WriteLine(e.Data[key].ToString()));
                throw;
            }

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [ClassData(typeof(SerializationData))]
        public void DeserializeTest(DistributedPropertySet data, ISettingsSerializer serializer)
        {
            IDistributedPropertySet result = null;

            try
            {
                result = serializer.Deserialize(serializer.Serialize(data));
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                e.Data.Keys.Cast<string>().ToList().ForEach(key => _output.WriteLine(e.Data[key].ToString()));
                throw;
            }

            Assert.NotNull(result);

            foreach (var key in data.Keys)
            {
                Assert.True(result.ContainsKey(key));
                _output.WriteLine($"{key}: {data[key]} {(data[key].Equals(result[key]) ? "==" : "!=")} {result[key]}");
                Assert.Equal(data[key], result[key]);
            }
        }
    }
}