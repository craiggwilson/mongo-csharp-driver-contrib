using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;

namespace MongoDB.Bson.Serialization
{
    public class JsonNetSerializationProvider : IBsonSerializationProvider
    {
        private readonly JsonSerializer _serializer;

        public JsonNetSerializationProvider(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public IBsonSerializer GetSerializer(Type type)
        {
            // We are going to handle everything except for BsonValue derivatives
            if(typeof(BsonValue).IsAssignableFrom(type))
            {
                return null;
            }

            return new JsonNetSerializer(_serializer);
        }
    }
}