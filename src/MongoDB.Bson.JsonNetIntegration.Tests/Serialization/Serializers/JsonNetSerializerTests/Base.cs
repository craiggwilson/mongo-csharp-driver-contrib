using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization.Serializers.JsonNetSerializerTests
{
    public abstract class Base
    {
        protected T DeserializeFromJson<T>(string json)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var subject = new JsonNetSerializer(serializer);

            using(var reader = JsonReader.Create(json))
            {
                return (T)subject.Deserialize(reader, typeof(T), null);
            }
        }

        protected string SerializeToJson<T>(T value)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var subject = new JsonNetSerializer(serializer);

            var sb = new StringBuilder();
            using(var sbWriter = new StringWriter(sb))
            using (var writer = new JsonWriter(sbWriter, JsonWriterSettings.Defaults))
            {
                subject.Serialize(writer, typeof(T), value, null);
            }

            return sb.ToString();
        }
    }
}