using System.Collections.Generic;
using NetExtensions.Object;
using Xunit;
using Xunit.Abstractions;

namespace NetExtensionsTests.Object {
    public class ToSensibleStringTests {
        private readonly ITestOutputHelper testOutputHelper;

        public ToSensibleStringTests(ITestOutputHelper testOutputHelper) {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void SensibleStringBoolean() {
            var value = true;
            Assert.Equal("True", value.ToSensibleString());
        }

        [Fact]
        public void SensibleStringNumber() {
            var value = 1;
            Assert.Equal("1", value.ToSensibleString());
        }

        [Fact]
        public void SensibleStringString() {
            var value = "value";
            Assert.Equal("value", value.ToSensibleString());
        }
        
        [Fact]
        public void SensibleStringNull() {
            object value = null;
            Assert.Null(value.ToSensibleString());
        }
        
        [Fact]
        public void SensibleStringArray() {
            var value = new [] {"one", "two"};
            Assert.Equal("[one, two]", value.ToSensibleString());
        }
        
        [Fact]
        public void SensibleStringOnGenericDictionary() {
            var value = new Dictionary<string, object> {["key"] = "value", ["key2"] = "value2"};
            Assert.Equal("{key: value, key2: value2}", value.ToSensibleString());
        }
        
        [Fact]
        public void SensibleStringNested() {
            var value = new Dictionary<string, object> {["key"] = new List<string>() {"one", "two"}};

            Assert.Equal("{key: [one, two]}", value.ToSensibleString());
        }
        
    }
}