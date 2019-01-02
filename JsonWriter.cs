using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Jannesen.FileFormat.Json
{
    public interface IJsonWriter
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

        private                 TextWriter              _textWriter;
        private                 bool                    _keepopen;
        private                 Stack<DomStatus>        _domStatus;

        public                                          JsonWriter(TextWriter textWriter): this(textWriter, true)
        {
        }
        public                                          JsonWriter(TextWriter textWriter, bool keepopen)
        {
            _textWriter = textWriter;
            _keepopen   = keepopen;
            _domStatus  = new Stack<DomStatus>();
            _domStatus.Push(DomStatus.BeginOfFile);
        }
        public                  void                    Dispose()
        {
            Flush();

            if (!_keepopen)
                _textWriter.Dispose();
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
        public                  void                    WriteNameValue(string name, string value)
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
        public                  void                    WriteNameValue(string name, IJsonWriter value)
        {
            WriteName(name);

            if (value != null)
                ((IJsonWriter)value).WriteTo(this);
            else
                WriteNull();
        }
        public                  void                    WriteNameValue<T>(string name, T[] value) where T: IJsonWriter
        {
            WriteName(name);
            WriteArray(value);
        }
        public                  void                    WriteNameValue<T>(string name, List<T> value) where T: IJsonWriter
        {
            WriteName(name);
            WriteArray(value);
        }
        public                  void                    WriteNameValue(string name, object value)
        {
            WriteName(name);
            WriteValue(value);
        }
        public                  void                    WriteValue(object value)
        {
            if (value == null                       ) { WriteNull   (                                 ); return; }
            if (value is string                     ) { WriteString ((string                    )value); return; }
            if (value is int                        ) { WriteInt    ((int                       )value); return; }
            if (value is byte                       ) { WriteInt    ((byte                      )value); return; }
            if (value is Int16                      ) { WriteInt    ((Int16                     )value); return; }
            if (value is Int32                      ) { WriteInt    ((Int32                     )value); return; }
            if (value is Int64                      ) { WriteInt    ((Int64                     )value); return; }
            if (value is decimal                    ) { WriteDecimal((decimal                   )value); return; }
            if (value is float                      ) { WriteDouble ((float                     )value); return; }
            if (value is double                     ) { WriteDouble ((double                    )value); return; }
            if (value is bool                       ) { WriteBool   ((bool                      )value); return; }
            if (value is IJsonWriter                ) { ((IJsonWriter)value).WriteTo(this); return;              }
            if (value is Dictionary<string, object> ) { WriteObject ((Dictionary<string, object>)value); return; }
            if (value is object[]                   ) { WriteArray  ((object[]                  )value); return; }
            if (value is List<object>               ) { WriteArray  ((List<object>              )value); return; }

            throw new ArgumentException("Invalid argument type " + value.GetType().FullName + ".");
        }
        public                  void                    WriteNull()
        {
            WriteRawValue("null");
        }
        public                  void                    WriteString(string value)
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
        public                  void                    WriteObject(Dictionary<string, object> value)
        {
            WriteStartObject();

            foreach(KeyValuePair<string, object> nameValue in value)
                WriteNameValue(nameValue.Key, nameValue.Value);

            WriteEndObject();
        }
        public                  void                    WriteArray(object[] array)
        {
            WriteStartArray();

            foreach(object obj in array)
                WriteValue(obj);

            WriteEndArray();
        }
        public                  void                    WriteArray<T>(T[] array) where T:IJsonWriter
        {
            WriteStartArray();

            if (array != null) {
                foreach (var i in array)
                    i.WriteTo(this);
            }

            WriteEndArray();
        }
        public                  void                    WriteArray<T>(List<T> array) where T:IJsonWriter
        {
            WriteStartArray();

            if (array != null) {
                foreach (var i in array)
                    i.WriteTo(this);
            }

            WriteEndArray();
        }
        public                  void                    WriteArray(List<object> array)
        {
            WriteStartArray();

            foreach(object obj in array)
                WriteValue(obj);

            WriteEndArray();
        }
        public                  void                    WriteName(string name)
        {
            _writeSeparator();
            _writeString(name);
            _textWriter.Write(":");
        }
        public                  void                    WriteRawValue(string snumber)
        {
            _writeSeparator();
            _textWriter.Write(snumber);
            _domStatus.Push(DomStatus.ValueWriten);
        }

        private                 void                    _writeString(string value)
        {
            _textWriter.Write('\"');

            value = value.Replace("\r\n", "\n");

            for(int i = 0 ; i < value.Length ; ++i) {
                char c = value[i];

                switch (c) {
                case '"':   _textWriter.Write("\\\"");      break;
                case '\\':  _textWriter.Write("\\\\");      break;
                case '\b':  _textWriter.Write("\\b");       break;
                case '\f':  _textWriter.Write("\\f");       break;
                case '\n':  _textWriter.Write("\\n");       break;
                case '\r':  _textWriter.Write("\\r");       break;
                case '\t':  _textWriter.Write("\\t");       break;

                default:
                    if (c >= 0x20)
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
            nibble = nibble & 0xF;

            return (nibble < 10) ? (char)((int)'0' + nibble) : (char)((int)'A' + (nibble - 10));
        }
    }
}
