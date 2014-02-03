using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MongoDB.Bson.IO
{
    public class JsonWriterMongoAdapter : Newtonsoft.Json.JsonWriter
    {
        private readonly BsonWriter _adaptee;

        public JsonWriterMongoAdapter(BsonWriter adaptee)
        {
            _adaptee = adaptee;
        }

        public override void Flush()
        {
            _adaptee.Flush();
        }

        public override void WriteEndArray()
        {
            _adaptee.WriteEndArray();
        }

        public override void WriteEndConstructor()
        {
            throw new NotSupportedException("MongoDB doesn't support JSON constructors.");
        }

        public override void WriteEndObject()
        {
            _adaptee.WriteEndDocument();
        }

        public override void WriteComment(string text)
        {
            throw new NotSupportedException("MongoDB doesn't support JSON comments.");
        }

        public override void WriteNull()
        {
            _adaptee.WriteNull();
        }

        public override void WritePropertyName(string name)
        {
            _adaptee.WriteName(name);
        }

        public override void WritePropertyName(string name, bool escape)
        {
            WritePropertyName(name);
        }

        public override void WriteRaw(string json)
        {
            throw new NotSupportedException("MongoDB doesn't support writing raw JSON.");
        }

        public override void WriteRawValue(string json)
        {
            throw new NotSupportedException("MongoDB doesn't support write raw JSON.");
        }

        public override void WriteStartArray()
        {
            _adaptee.WriteStartArray();
        }

        public override void WriteStartConstructor(string name)
        {
            throw new NotSupportedException("MongoDB doesn't support JSON constructors.");
        }

        public override void WriteStartObject()
        {
            _adaptee.WriteStartDocument();
        }

        public override void WriteUndefined()
        {
            _adaptee.WriteUndefined();
        }

        public override void WriteValue(bool value)
        {
            _adaptee.WriteBoolean(value);
        }

        public override void WriteValue(byte value)
        {
            _adaptee.WriteBytes(new[] { value });
        }

        public override void WriteValue(byte[] value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                _adaptee.WriteBytes(value);
            }
        }

        public override void WriteValue(char value)
        {
            _adaptee.WriteString(new String(new[] { value }));
        }

        public override void WriteValue(DateTime value)
        {
            var bsonDateTime = new MongoDB.Bson.BsonDateTime(value);
            _adaptee.WriteDateTime(bsonDateTime.MillisecondsSinceEpoch);
        }

        public override void WriteValue(DateTimeOffset value)
        {
            throw new NotSupportedException("MongoDB doesn't support DateTimeOffset.");
        }

        public override void WriteValue(decimal value)
        {
            WriteValue((double)value);
        }

        public override void WriteValue(double value)
        {
            _adaptee.WriteDouble(value);
        }

        public override void WriteValue(float value)
        {
            _adaptee.WriteDouble(value);
        }

        public override void WriteValue(Guid value)
        {
            var binaryData = new MongoDB.Bson.BsonBinaryData(value, MongoDB.Bson.GuidRepresentation.Standard);
            _adaptee.WriteBinaryData(binaryData);
        }

        public override void WriteValue(int value)
        {
            _adaptee.WriteInt32(value);
        }

        public override void WriteValue(long value)
        {
            _adaptee.WriteInt64(value);
        }

        public override void WriteValue(sbyte value)
        {
            _adaptee.WriteInt32(value);
        }

        public override void WriteValue(short value)
        {
            _adaptee.WriteInt32(value);
        }

        public override void WriteValue(string value)
        {
            _adaptee.WriteString(value);
        }

        public override void WriteValue(TimeSpan value)
        {
            WriteValue(value.Milliseconds);
        }

        public override void WriteValue(uint value)
        {
            _adaptee.WriteInt32((int)value);
        }

        public override void WriteValue(ulong value)
        {
            _adaptee.WriteInt64((long)value);
        }

        public override void WriteValue(Uri value)
        {
            WriteValue(value.ToString());
        }

        public override void WriteValue(ushort value)
        {
            WriteValue((int)value);
        }

        public override void WriteWhitespace(string ws)
        {
            throw new NotSupportedException("MongoDB doesn't support JSON whitespace.");
        }

        protected override void WriteValueDelimiter()
        {
            throw new NotSupportedException("MongoDB doesn't support JSON delimiters.");
        }
    }
}