using Xunit;
using GPS.SettingsSync;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GPS.SettingsSync.Tests
{
    public class DistributedPropertySetTests
    {
        [Fact]
        public void NullNameTest()
        {
            var set = new DistributedPropertySet();
            Assert.Throws<ArgumentNullException>((Action)(() => set.TryAdd(null, null)));
        }

        [Fact]
        public void NameNotFoundIndexerTest()
        {
            var set = new DistributedPropertySet();

            Assert.Throws<KeyNotFoundException>((Action)(() => _ = set["string"]));
        }

        [Fact]
        public void NameNotFoundTryGetTest()
        {
            var set = new DistributedPropertySet();

            Assert.False(set.TryGetValue("string", out _));
        }

        [Theory()]
        [InlineData("string", "0")]
        [InlineData("int", int.MaxValue)]
        [InlineData("uint", uint.MaxValue)]
        [InlineData("double", double.MaxValue)]
        [InlineData("float", float.MaxValue)]
        [InlineData("bool", true)]
        public void TryAddTest(string name, object value)
        {
            var flag = false;
            var set = new DistributedPropertySet();
            set.MapChanged += (sender, args) =>
            {
                if (args.DistributedCollectionChange == DistributedCollectionChange.ItemInserted)
                {
                    flag = true;
                }
            };

            Assert.True(set.TryAdd(name, value));
            Assert.Equal(value, set[name]);
            Assert.True(flag);
        }

        [Theory()]
        [InlineData("string", "0")]
        [InlineData("int", int.MaxValue)]
        [InlineData("uint", uint.MaxValue)]
        [InlineData("double", double.MaxValue)]
        [InlineData("float", float.MaxValue)]
        [InlineData("bool", true)]
        public void TryRemoveTest(string name, object value)
        {
            var flag = false;
            var set = new DistributedPropertySet();
            set.MapChanged += (sender, args) =>
            {
                if (args.DistributedCollectionChange == DistributedCollectionChange.ItemRemoved)
                {
                    flag = true;
                }
            };

            Assert.True(set.TryAdd(name, value));
            Assert.True(set.TryRemove(name, out var outValue));
            Assert.Equal(value, outValue);
            Assert.False(set.ContainsKey(name));
            Assert.True(flag);
        }

        [Theory()]
        [InlineData("string", "0", "1")]
        [InlineData("int", int.MaxValue, int.MinValue)]
        [InlineData("uint", uint.MaxValue, uint.MinValue)]
        [InlineData("double", double.MaxValue, double.MinValue)]
        [InlineData("float", float.MaxValue, float.MinValue)]
        [InlineData("bool", true, false)]
        public void TryUpdateTest(string name, object value, object updatedValue)
        {
            var flag = false;
            var set = new DistributedPropertySet();
            set.MapChanged += (sender, args) =>
            {
                if (args.DistributedCollectionChange == DistributedCollectionChange.ItemChanged)
                {
                    flag = true;
                }
            };

            Assert.True(set.TryAdd(name, value));
            Assert.True(set.TryUpdate(name, updatedValue, value));
            Assert.Equal(updatedValue, set[name]);
            Assert.True(flag);
        }

        [Fact()]
        public void ClearTest()
        {
            var set = new DistributedPropertySet();

            set.TryAdd("string", "0");
            set.TryAdd("int", int.MaxValue);
            set.TryAdd("uint", uint.MaxValue);
            set.TryAdd("double", double.MaxValue);

            Assert.NotEmpty(set);

            set.Clear();

            Assert.Empty(set);
        }

        [Theory()]
        [InlineData("string", "0")]
        [InlineData("int", int.MaxValue)]
        [InlineData("uint", uint.MaxValue)]
        [InlineData("double", double.MaxValue)]
        [InlineData("float", float.MaxValue)]
        [InlineData("bool", true)]
        public void AddOrUpdateTest_Add(string name, object value)
        {
            var flag = false;
            var set = new DistributedPropertySet();
            set.MapChanged += (sender, args) =>
            {
                Assert.Equal(DistributedCollectionChange.ItemInserted, args.DistributedCollectionChange);
                flag = true;
            };

            Assert.Equal(value, set.AddOrUpdate(name, value, null));
            Assert.True(flag);
        }

        [Theory()]
        [InlineData("string", "0")]
        [InlineData("int", int.MaxValue)]
        [InlineData("uint", uint.MaxValue)]
        [InlineData("double", double.MaxValue)]
        [InlineData("float", float.MaxValue)]
        [InlineData("bool", true)]
        public void AddOrUpdateTest_AddFactory(string name, object value)
        {
            var flag = false;
            var set = new DistributedPropertySet();
            set.MapChanged += (sender, args) =>
            {
                Assert.Equal(DistributedCollectionChange.ItemInserted, args.DistributedCollectionChange);
                flag = true;
            };

            Assert.Equal(value, set.AddOrUpdate(name, n => value, null));
            Assert.True(flag);
        }

        [Theory()]
        [InlineData("string", "0", "1")]
        [InlineData("int", int.MaxValue, int.MinValue)]
        [InlineData("uint", uint.MaxValue, uint.MinValue)]
        [InlineData("double", double.MaxValue, double.MinValue)]
        [InlineData("float", float.MaxValue, float.MinValue)]
        [InlineData("bool", true, false)]
        public void AddOrUpdateTest_Update(string name, object addValue, object updateValue)
        {
            var flag = false;
            var set = new DistributedPropertySet();
            set.MapChanged += (sender, args) =>
            {
                if (args.DistributedCollectionChange == DistributedCollectionChange.ItemChanged)
                {
                    flag = true;
                }
            };

            Assert.True(set.TryAdd(name, addValue));
            Assert.Equal(updateValue, set.AddOrUpdate(name, addValue, (n, v) => updateValue));
            Assert.True(flag);
        }

        [Theory()]
        [InlineData("string", "0", "1")]
        [InlineData("int", int.MaxValue, int.MinValue)]
        [InlineData("uint", uint.MaxValue, uint.MinValue)]
        [InlineData("double", double.MaxValue, double.MinValue)]
        [InlineData("float", float.MaxValue, float.MinValue)]
        [InlineData("bool", true, false)]
        public void AddOrUpdateTest_UpdateFactory(string name, object addValue, object updateValue)
        {
            var flag = false;
            var set = new DistributedPropertySet();
            set.MapChanged += (sender, args) =>
            {
                if (args.DistributedCollectionChange == DistributedCollectionChange.ItemChanged)
                {
                    flag = true;
                }
            };

            Assert.True(set.TryAdd(name, addValue));
            Assert.Equal(updateValue, set.AddOrUpdate(name, n => addValue, (n, v) => updateValue));
            Assert.True(flag);
        }

        [Theory()]
        [InlineData("string", "0")]
        [InlineData("int", int.MaxValue)]
        [InlineData("uint", uint.MaxValue)]
        [InlineData("double", double.MaxValue)]
        [InlineData("float", float.MaxValue)]
        [InlineData("bool", true)]
        public void GetOrAddTest_Get(string name, object value)
        {
            var flag = false;
            var set = new DistributedPropertySet();
            set.MapChanged += (sender, args) =>
            {
                if (args.DistributedCollectionChange == DistributedCollectionChange.ItemInserted)
                {
                    flag = true;
                }
            };

            Assert.True(set.TryAdd(name, value));
            flag = false;

            Assert.Equal(value, set.GetOrAdd(name, null));
            Assert.False(flag);
        }

        [Theory()]
        [InlineData("string", "0")]
        [InlineData("int", int.MaxValue)]
        [InlineData("uint", uint.MaxValue)]
        [InlineData("double", double.MaxValue)]
        [InlineData("float", float.MaxValue)]
        [InlineData("bool", true)]
        public void GetOrAddTest_AddValue(string name, object value)
        {
            var flag = false;
            var set = new DistributedPropertySet();
            set.MapChanged += (sender, args) =>
            {
                if (args.DistributedCollectionChange == DistributedCollectionChange.ItemInserted)
                {
                    flag = true;
                }
            };

            Assert.Equal(value, set.GetOrAdd(name, value));
            Assert.Equal(value, set.TryGetValue(name, out var outValue) ? outValue : null);
            Assert.True(flag);
        }

        [Theory()]
        [InlineData("string", "0")]
        [InlineData("int", int.MaxValue)]
        [InlineData("uint", uint.MaxValue)]
        [InlineData("double", double.MaxValue)]
        [InlineData("float", float.MaxValue)]
        [InlineData("bool", true)]
        public void GetOrAddTest_AddFactory(string name, object value)
        {
            var flag = false;
            var set = new DistributedPropertySet();
            set.MapChanged += (sender, args) =>
            {
                if (args.DistributedCollectionChange == DistributedCollectionChange.ItemInserted)
                {
                    flag = true;
                }
            };

            Assert.Equal(value, set.GetOrAdd(name, n => value));
            Assert.Equal(value, set.TryGetValue(name, out var outValue) ? outValue : null);
            Assert.True(flag);
        }

        [Theory()]
        [InlineData("string", "0")]
        [InlineData("int", int.MaxValue)]
        [InlineData("uint", uint.MaxValue)]
        [InlineData("double", double.MaxValue)]
        [InlineData("float", float.MaxValue)]
        [InlineData("bool", true)]
        public void GetOrAddTest_AddFactoryWithArgument(string name, object value)
        {
            var flag = false;
            var set = new DistributedPropertySet();
            set.MapChanged += (sender, args) =>
            {
                if (args.DistributedCollectionChange == DistributedCollectionChange.ItemInserted)
                {
                    flag = true;
                }
            };

            Assert.Equal(value, set.GetOrAdd(name, (n, arg) => arg, value));
            Assert.Equal(value, set.TryGetValue(name, out var outValue) ? outValue : null);
            Assert.True(flag);
        }
    }
}