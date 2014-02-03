using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;

namespace MongoDB.Bson.Serialization.Serializers.JsonNetSerializerTests
{
    [TestFixture]
    public class WithConverter : Base
    {
        [Test]
        public void Should_serialize_with_a_custom_converter()
        {
            var foo = new Foo { A = 42 };

            var json = SerializeToJson(foo);
            var expected = "{ \"A\" : 420 }";

            json.ShouldBe(expected);
        }

        [Test]
        public void Should_deserialize_with_a_custom_converter()
        {
            var json = "{ \"A\" : 420 }";

            var foo = DeserializeFromJson<Foo>(json);

            foo.A.ShouldBe(42);
        }

        private class Foo
        {
            [JsonConverter(typeof(IntTimes10Converter))]
            public int A { get; set; }
        }

        private class IntTimes10Converter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
 	            return objectType == typeof(int);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return (int)(((int)reader.Value) / 10);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
 	            writer.WriteValue(((int)value) * 10);
            }
        }
    }
}
