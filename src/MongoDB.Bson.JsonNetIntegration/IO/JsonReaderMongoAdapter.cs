using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using JToken = Newtonsoft.Json.JsonToken;

namespace MongoDB.Bson.IO
{
    public class JsonReaderMongoAdapter : Newtonsoft.Json.JsonReader
    {
        private enum ContainerType { Array, Document };

        private readonly BsonReader _adaptee;
        private readonly DateTimeKind _dateTimeKindHandling;

        public JsonReaderMongoAdapter(BsonReader adaptee)
            : this(adaptee, DateTimeKind.Utc)
        {
        }

        public JsonReaderMongoAdapter(BsonReader adaptee, DateTimeKind dateTimeKindHandling)
        {
            _adaptee = adaptee;
            _dateTimeKindHandling = dateTimeKindHandling;
        }

        public override bool Read()
        {
            switch (_adaptee.State)
            {
                case IO.BsonReaderState.Done:
                    return false;
                case IO.BsonReaderState.Name:
                    SetToken(JToken.PropertyName, _adaptee.ReadName());
                    return true;
                case IO.BsonReaderState.Initial:
                case IO.BsonReaderState.Type:
                    _adaptee.ReadBsonType();
                    return Read();
                case IO.BsonReaderState.EndOfArray:
                    _adaptee.ReadEndArray();
                    SetToken(JToken.EndArray);
                    return true;
                case IO.BsonReaderState.EndOfDocument:
                    _adaptee.ReadEndDocument();
                    SetToken(JToken.EndObject);
                    return true;
                case IO.BsonReaderState.Value:
                    ReadValue(_adaptee.GetCurrentBsonType());
                    return true;
                default:
                    throw new NotSupportedException("Unsupported state " + _adaptee.State);
            }
        }

        private void ReadValue(BsonType bsonType)
        {
            switch (bsonType)
            {
                case BsonType.Array:
                    _adaptee.ReadStartArray();
                    SetToken(JToken.StartArray);
                    break;
                case BsonType.Binary:
                    var bin = _adaptee.ReadBinaryData();
                    SetToken(JToken.Bytes, bin.Bytes);
                    break;
                case BsonType.Boolean:
                    SetToken(JToken.Boolean, _adaptee.ReadBoolean());
                    break;
                case BsonType.DateTime:
                    var millisecondsSinceEpoch = _adaptee.ReadDateTime();
                    var dt = BsonUtils.ToDateTimeFromMillisecondsSinceEpoch(millisecondsSinceEpoch);

                    switch (_dateTimeKindHandling)
                    {
                        case DateTimeKind.Local:
                            dt = dt.ToLocalTime();
                            break;
                        case DateTimeKind.Unspecified:
                            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
                            break;
                    }
                    SetToken(JToken.Date, dt);
                    break;
                case BsonType.Document:
                    _adaptee.ReadStartDocument();
                    SetToken(JToken.StartObject);
                    break;
                case BsonType.Double:
                    var d = _adaptee.ReadDouble();
                    if (FloatParseHandling == FloatParseHandling.Decimal)
                        SetToken(JToken.Float, Convert.ToDecimal(d, CultureInfo.InvariantCulture));
                    else
                        SetToken(JToken.Float, d);
                    break;
                case BsonType.Int32:
                    SetToken(JToken.Integer, _adaptee.ReadInt32());
                    break;
                case BsonType.Int64:
                    SetToken(JToken.Integer, _adaptee.ReadInt64());
                    break;
                case BsonType.JavaScript:
                    SetToken(JToken.String, _adaptee.ReadJavaScript());
                    break;
                case BsonType.JavaScriptWithScope:
                    SetToken(JToken.String, _adaptee.ReadJavaScriptWithScope());
                    break;
                case BsonType.MinKey:
                    break;
                case BsonType.MaxKey:
                    break;
                case BsonType.Null:
                    _adaptee.ReadNull();
                    SetToken(JToken.Null);
                    break;
                case BsonType.ObjectId:
                    var oid = _adaptee.ReadObjectId();
                    SetToken(JToken.Bytes, oid.ToByteArray());
                    break;
                case BsonType.RegularExpression:
                    var regex = _adaptee.ReadRegularExpression();
                    var r = "/" + regex.Pattern + "/" + regex.Options;
                    SetToken(JToken.String, r);
                    break;
                case BsonType.String:
                    SetToken(JToken.String, _adaptee.ReadString());
                    break;
                case BsonType.Symbol:
                    SetToken(JToken.String, _adaptee.ReadSymbol());
                    break;
                case BsonType.Timestamp:
                    SetToken(JToken.Integer, _adaptee.ReadTimestamp());
                    break;
                case BsonType.Undefined:
                    _adaptee.ReadUndefined();
                    SetToken(JToken.Undefined);
                    break;
                default:
                    throw new NotSupportedException("Unsupported BsonType " + bsonType);
            }
        }

        public override byte[] ReadAsBytes()
        {
            if (!Read())
            {
                SetToken(JToken.None);
                return null;
            }

            if (IsWrappedInTypeObject())
            {
                var bytes = ReadAsBytes();
                Read(); // end object...
                SetToken(JToken.Bytes, bytes);
            }

            switch (TokenType)
            {
                case JToken.EndArray:
                case JToken.Null:
                    return null;
                case JToken.Bytes:
                    return (byte[])Value;
                case JToken.StartArray:
                    var data = new List<byte>();

                    while (Read())
                    {
                        switch (TokenType)
                        {
                            case JToken.Integer:
                                data.Add(Convert.ToByte(Value, CultureInfo.InvariantCulture));
                                break;
                            case JToken.EndArray:
                                byte[] d = data.ToArray();
                                SetToken(JToken.Bytes, d);
                                return d;
                            default:
                                throw new JsonReaderException(string.Format(CultureInfo.InvariantCulture, "Error reading bytes. Unexpected token: {0}.", Value));
                        }
                    }
                    throw new JsonReaderException("Unexpected end when reading bytes.");
                default:
                    throw new JsonReaderException(string.Format(CultureInfo.InvariantCulture, "Error reading bytes. Unexpected token: {0}.", Value));
            }
        }

        public override DateTime? ReadAsDateTime()
        {
            if (!Read())
            {
                SetToken(JToken.None);
                return null;
            }

            switch (TokenType)
            {
                case JToken.Date:
                    return (DateTime)Value;
                case JToken.EndArray:
                case JToken.Null:
                    return null;
                case JToken.String:
                    var s = (string)Value;
                    if (string.IsNullOrEmpty(s))
                    {
                        SetToken(JToken.Null);
                        return null;
                    }

                    DateTime dt;
                    if (DateTime.TryParse(s, Culture, DateTimeStyles.RoundtripKind, out dt))
                    {
                        dt = EnsureDateTime(dt, DateTimeZoneHandling);
                        SetToken(JToken.Date, dt);
                        return dt;
                    }
                    throw new JsonReaderException(string.Format(CultureInfo.InvariantCulture, "Could not convert string to DateTime: {0}.", Value));
                default:
                    throw new JsonReaderException(string.Format(CultureInfo.InvariantCulture, "Error reading DateTime. Unexpected token: {0}.", Value));
            }
        }

        public override DateTimeOffset? ReadAsDateTimeOffset()
        {
            throw new NotSupportedException("MongoDB doesn't support DateTimeOffset.");
        }

        public override decimal? ReadAsDecimal()
        {
            if (!Read())
            {
                return null;
            }

            switch (TokenType)
            {
                case JToken.Integer:
                case JToken.Float:
                    SetToken(JToken.Float, Convert.ToDecimal(Value, CultureInfo.InvariantCulture));
                    return (decimal)Value;
                case JToken.EndArray:
                case JToken.Null:
                    return null;
                case JToken.String:
                    var s = (string)Value;
                    if (string.IsNullOrEmpty(s))
                    {
                        SetToken(JToken.Null);
                        return null;
                    }
                    decimal d;
                    if (decimal.TryParse(s, NumberStyles.Number, Culture, out d))
                    {
                        SetToken(JToken.Float, d);
                        return d;
                    }

                    throw new JsonReaderException(string.Format(CultureInfo.InvariantCulture, "Could not convert string to decimal: {0}.", Value));
                default:
                    throw new JsonReaderException(string.Format(CultureInfo.InvariantCulture, "Error reading decimal. Unexpected token: {0}.", Value));
            }
        }

        public override int? ReadAsInt32()
        {
            if (!Read())
            {
                SetToken(JToken.None);
                return null;
            }

            switch (TokenType)
            {
                case JToken.Float:
                    SetToken(JToken.Integer, Convert.ToInt32(Value, CultureInfo.InvariantCulture));
                    return (int)Value;
                case JToken.Integer:
                    return (int)Value;
                case JToken.EndArray:
                case JToken.Null:
                    return null;
                case JToken.String:
                    var s = (string)Value;
                    if (string.IsNullOrEmpty(s))
                    {
                        SetToken(JToken.Null);
                        return null;
                    }

                    int i;
                    if (int.TryParse(s, NumberStyles.Integer, Culture, out i))
                    {
                        SetToken(JToken.Integer, i);
                        return i;
                    }

                    throw new JsonReaderException(string.Format(CultureInfo.InvariantCulture, "Could not convert string to integer: {0}.", Value));
                default:
                    throw new JsonReaderException(string.Format(CultureInfo.InvariantCulture, "Error reading integer. Unexpected token: {0}.", Value));
            }
        }

        public override string ReadAsString()
        {
            if (!Read())
            {
                SetToken(JToken.None);
                return null;
            }

            switch (TokenType)
            {
                case JToken.String:
                    return (string)Value;
                case JToken.EndArray:
                case JToken.Null:
                    return null;
                default:
                    if (IsPrimitive(TokenType))
                    {
                        string s;
                        if (Value is IFormattable)
                        {
                            s = ((IFormattable)Value).ToString(null, Culture);
                        }
                        else
                        {
                            s = Value.ToString();
                        }

                        SetToken(JToken.String, s);
                        return s;
                    }

                    throw new JsonReaderException(string.Format(CultureInfo.InvariantCulture, "Error reading string. Unexpected token: {0}.", Value));
            }
        }

        private bool IsWrappedInTypeObject()
        {
            if (TokenType == JToken.StartObject)
            {
                if (!Read())
                    throw new JsonReaderException("Unexpected end when reading bytes.");

                if (Value.ToString() == "$type")
                {
                    Read();
                    if (Value != null && Value.ToString().StartsWith("System.Byte[]"))
                    {
                        Read();
                        if (Value.ToString() == "$value")
                        {
                            return true;
                        }
                    }
                }

                throw new JsonReaderException(string.Format(CultureInfo.InvariantCulture, "Error reading bytes. Unexpected token: {0}.", JToken.StartObject));
            }

            return false;
        }

        private static bool IsPrimitive(JToken token)
        {
            switch (token)
            {
                case JToken.Integer:
                case JToken.Float:
                case JToken.String:
                case JToken.Boolean:
                case JToken.Undefined:
                case JToken.Null:
                case JToken.Date:
                case JToken.Bytes:
                    return true;
                default:
                    return false;
            }
        }

        private static DateTime EnsureDateTime(DateTime value, DateTimeZoneHandling timeZone)
        {
            switch (timeZone)
            {
                case DateTimeZoneHandling.Local:
                    value = SwitchToLocalTime(value);
                    break;
                case DateTimeZoneHandling.Utc:
                    value = SwitchToUtcTime(value);
                    break;
                case DateTimeZoneHandling.Unspecified:
                    value = new DateTime(value.Ticks, DateTimeKind.Unspecified);
                    break;
                case DateTimeZoneHandling.RoundtripKind:
                    break;
                default:
                    throw new ArgumentException("Invalid date time handling value.");
            }

            return value;
        }

        private static DateTime SwitchToLocalTime(DateTime value)
        {
            switch (value.Kind)
            {
                case DateTimeKind.Unspecified:
                    return new DateTime(value.Ticks, DateTimeKind.Local);

                case DateTimeKind.Utc:
                    return value.ToLocalTime();

                case DateTimeKind.Local:
                    return value;
            }
            return value;
        }

        private static DateTime SwitchToUtcTime(DateTime value)
        {
            switch (value.Kind)
            {
                case DateTimeKind.Unspecified:
                    return new DateTime(value.Ticks, DateTimeKind.Utc);

                case DateTimeKind.Utc:
                    return value;

                case DateTimeKind.Local:
                    return value.ToUniversalTime();
            }
            return value;
        }
    }
}