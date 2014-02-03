using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.IO;
using Newtonsoft.Json;

namespace MongoDB.Bson.Serialization.Serializers
{
    public class JsonNetSerializer : BsonBaseSerializer
    {
        private readonly JsonSerializer _serializer;

        public JsonNetSerializer(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
        {
            var jsonNetReader = new JsonReaderMongoAdapter(bsonReader);
            return _serializer.Deserialize(jsonNetReader, nominalType);
        }

        public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            var jsonNetWriter = new JsonWriterMongoAdapter(bsonWriter);
            _serializer.Serialize(jsonNetWriter, value);
        }
    }
}