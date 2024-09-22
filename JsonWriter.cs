using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Jannesen.FileFormat.Json
{
    public interface IJsonSerializer
    {
        void            WriteTo(JsonWriter writer);
    }

    public sealed class JsonWriter: IDisposable
    {
        private enum DomStatus
        {
            BeginOfFile     = 0,
            ObjectStart,
            ArrayStart,
            ValueWriten
        }

        private readonly        TextWriter              _textWriter;
        private readonly        bool                    _keepopen;
        private readonly        bool                    _ascii;
        private readonly        Stack<DomStatus>        _domStatus;

        public                                          JsonWriter(TextWriter textWriter): this(textWriter, true)
        {
        }
        public                                          JsonWriter(TextWriter textWriter, bool keepopen): this(textWriter, keepopen, false)
        {
        }
        public                                          JsonWriter(TextWriter textWriter, bool keepopen, bool ascii)
        {
            _textWriter = textWriter;
            _keepopen   = keepopen;
            _ascii      = ascii;
            _domStatus  = new Stack<DomStatus>();
            _domStatus.Push(DomStatus.BeginOfFile);
        }
        public                  void                    Dispose()
        {
            Flush();

            if (!_keepopen)
                _textWriter.Dispose();
        }

        public static          string                   Stringify(object value, bool ascii=false)
        {
            using (var stringWriter = new StringWriter()) {
                using (var jsonWriter = new JsonWriter(stringWriter, true, ascii)) {
                    jsonWriter.WriteValue(value);
                }

                return stringWriter.ToString();
            }
        }
        public static           string                  Serialize(IJsonSerializer obj, bool ascii=false)
        {
            using (var stringWriter = new StringWriter()) {
                using (var jsonWriter = new JsonWriter(stringWriter, true, ascii)) {
                    jsonWriter.WriteSerialize(obj);
                }
                return stringWriter.ToString();
            }
        }

        public                  void                    Flush()
        {
            while (_domStatus.Count > 0) {
                switch(_domStatus.Pop()) {
                case DomStatus.ObjectStart:     _textWriter.Write("}");     break;
                case DomStatus.ArrayStart:      _textWriter.Write("]");     break;
                }
            }

            _textWriter.Flush();
        }
        public                  void                    WriteStartObject()
        {
            _writeSeparator();
            _textWriter.Write("{");
            _domStatus.Push(DomStatus.ObjectStart);
        }
        public                  void                    WriteStartObject(string name)
        {
            WriteName(name);
            _textWriter.Write("{");
            _domStatus.Push(DomStatus.ObjectStart);
        }
        public                  void                    WriteEndObject()
        {
            if (_domStatus.Peek() == DomStatus.ValueWriten)
                _domStatus.Pop();

            if (_domStatus.Pop() != DomStatus.ObjectStart)
                throw new InvalidOperationException("WriteEndObject not posible.");

            _textWriter.Write("}");
            _domStatus.Push(DomStatus.ValueWriten);
        }
        public                  void                    WriteStartArray()
        {
            _writeSeparator();
            _textWriter.Write("[");
            _domStatus.Push(DomStatus.ArrayStart);
        }
        public                  void                    WriteStartArray(string name)
        {
            WriteName(name);
            _textWriter.Write("[");
            _domStatus.Push(DomStatus.ArrayStart);
        }
        public                  void                    WriteEndArray()
        {
            if (_domStatus.Peek() == DomStatus.ValueWriten)
                _domStatus.Pop();

            if (_domStatus.Pop() != DomStatus.ArrayStart)
                throw new InvalidOperationException("WriteEndObject not posible.");

            _textWriter.Write("]");
            _domStatus.Push(DomStatus.ValueWriten);
        }
        public                  void                    WriteNameValue(string name, string? value)
        {
            WriteName(name);
            if (value != null)
                WriteString(value);
            else
                WriteNull();
        }
        public                  void                    WriteNameValue(string name, int value)
        {
            WriteName(name);
            WriteInt(value);
        }
        public                  void                    WriteNameValue(string name, Int64 value)
        {
            WriteName(name);
            WriteInt(value);
        }
        public                  void                    WriteNameValue(string name, bool value)
        {
            WriteName(name);
            WriteBool(value);
        }
        public                  void                    WriteNameValue(string name, decimal value)
        {
            WriteName(name);
            WriteDecimal(value);
        }
        public                  void                    WriteNameValue(string name, double value)
        {
            WriteName(name);
            WriteDouble(value);
        }
        public                  void                    WriteNameValue(string name, DateTime value)
        {
            WriteName(name);
            WriteDateTime(value);
        }
        public                  void                    WriteNameValue(string name, IJsonSerializer value)
        {
            WriteName(name);
            WriteSerialize(value);
        }
        public                  void                    WriteNameValue<T>(string name, T[] value) where T: IJsonSerializer
        {
            WriteName(name);
            WriteArray(value);
        }
        public                  void                    WriteNameValue<T>(string name, IReadOnlyCollection<T> value) where T: IJsonSerializer
        {
            WriteName(name);
            WriteArray(value);
        }
        public                  void                    WriteNameValue(string name, object? value)
        {
            WriteName(name);
            WriteValue(value);
        }
        public                  void                    WriteValue(object? value)
        {
            if (value == null                        )                   { WriteNull    ();                      return; }
            if (value is string                      string_value)       { WriteString  (string_value);          return; }
            if (value is int                         int_value)          { WriteInt     (int_value);             return; }
            if (value is byte                        byte_value)         { WriteInt     (byte_value);            return; }
            if (value is Int16                       int16_value)        { WriteInt     (int16_value);           return; }
            if (value is Int32                       int32_value)        { WriteInt     (int32_value);           return; }
            if (value is Int64                       int64_value)        { WriteInt     (int64_value);           return; }
            if (value is decimal                     decimal_value)      { WriteDecimal (decimal_value);         return; }
            if (value is float                       float_value)        { WriteDouble  (float_value);           return; }
            if (value is double                      double_value)       { WriteDouble  (double_value);          return; }
            if (value is bool                        bool_value)         { WriteBool    (bool_value);            return; }
            if (value is DateTime                    datetime_value)     { WriteDateTime(datetime_value);        return; }
            if (value is IJsonSerializer             jsonwrite_value)    { jsonwrite_value.WriteTo(this);        return; }
            if (value is Dictionary<string, object?> dictionay_value)    { WriteObject  (dictionay_value);       return; }
            if (value is object[]                    objectarray_value)  { WriteArray   (objectarray_value);     return; }
            if (value is List<object>                listobject_value)   { WriteArray   (listobject_value);      return; }

            throw new ArgumentException("Invalid argument type " + value.GetType().FullName + ".");
        }
        public                  void                    WriteNull()
        {
            WriteRawValue("null");
        }
        public                  void                    WriteString(string? value)
        {
            if (value != null) {
                _writeSeparator();
                _writeString(value);
                _domStatus.Push(DomStatus.ValueWriten);
            }
            else
                WriteNull();
        }
        public                  void                    WriteInt(Int64 value)
        {
            WriteRawValue(value.ToString(CultureInfo.InvariantCulture));
        }
        public                  void                    WriteDecimal(decimal value)
        {
            WriteRawValue(value.ToString(CultureInfo.InvariantCulture));
        }
        public                  void                    WriteDouble(double value)
        {
            WriteRawValue(value.ToString("G", CultureInfo.InvariantCulture));
        }
        public                  void                    WriteBool(bool value)
        {
            WriteRawValue(value ? "true" : "false");
        }
        public                  void                    WriteDateTime(DateTime value)
        {
            WriteString(value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture));
        }
        public                  void                    WriteObject(Dictionary<string, object?> value)
        {
            ArgumentNullException.ThrowIfNull(value);

            WriteStartObject();

            foreach(var nameValue in value)
                WriteNameValue(nameValue.Key, nameValue.Value);

            WriteEndObject();
        }
        public                  void                    WriteArray(object[] array)
        {
            ArgumentNullException.ThrowIfNull(array);

            WriteStartArray();

            foreach(var obj in array)
                WriteValue(obj);

            WriteEndArray();
        }
        public                  void                    WriteArray<T>(T[] array) where T:IJsonSerializer
        {
            ArgumentNullException.ThrowIfNull(array);

            WriteStartArray();

            if (array != null) {
                foreach (var i in array)
                    i.WriteTo(this);
            }

            WriteEndArray();
        }
        public                  void                    WriteArray<T>(IReadOnlyCollection<T> array) where T:IJsonSerializer
        {
            ArgumentNullException.ThrowIfNull(array);

            WriteStartArray();

            if (array != null) {
                foreach (var i in array)
                    i.WriteTo(this);
            }

            WriteEndArray();
        }
        public                  void                    WriteArray(IReadOnlyCollection<object?> array)
        {
            ArgumentNullException.ThrowIfNull(array);

            WriteStartArray();

            foreach(var obj in array)
                WriteValue(obj);

            WriteEndArray();
        }
        public                  void                    WriteName(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            _writeSeparator();
            _writeString(name);
            _textWriter.Write(":");
        }
        public                  void                    WriteRawValue(string rawvalue)
        {
            ArgumentNullException.ThrowIfNull(rawvalue);

            _writeSeparator();
            _textWriter.Write(rawvalue);
            _domStatus.Push(DomStatus.ValueWriten);
        }
        public                  void                    WriteSerialize(IJsonSerializer? value)
        {
            if (value == null) {
                WriteNull();
            }
            else {
                value.WriteTo(this);
            }
        }

        private                 void                    _writeString(string value)
        {
            _textWriter.Write('\"');

            value = value.Replace("\r\n", "\n", StringComparison.Ordinal);

            for(var i = 0 ; i < value.Length ; ++i) {
                var c = value[i];

                switch (c) {
                case '"':   _textWriter.Write("\\\"");      break;
                case '\\':  _textWriter.Write("\\\\");      break;
                case '\b':  _textWriter.Write("\\b");       break;
                case '\f':  _textWriter.Write("\\f");       break;
                case '\n':  _textWriter.Write("\\n");       break;
                case '\r':  _textWriter.Write("\\r");       break;
                case '\t':  _textWriter.Write("\\t");       break;

                default:
                    if (c >= 0x20 && (c <= 0x7f || !_ascii))
                        _textWriter.Write(c);
                    else {
                        _textWriter.Write("\\u");
                        _textWriter.Write(_nibbleToHex(c >> 12));
                        _textWriter.Write(_nibbleToHex(c >>  8));
                        _textWriter.Write(_nibbleToHex(c >>  4));
                        _textWriter.Write(_nibbleToHex(c >>  0));
                    }
                    break;
                }
            }

            _textWriter.Write('\"');
        }
        private                 void                    _writeSeparator()
        {
            if (_domStatus.Peek() == DomStatus.ValueWriten) {
                _domStatus.Pop();
                _textWriter.Write(',');
            }
        }
        private     static      char                    _nibbleToHex(int nibble)
        {
            nibble &= 0xF;

            return (nibble < 10) ? (char)((int)'0' + nibble) : (char)((int)'A' + (nibble - 10));
        }
    }
}
