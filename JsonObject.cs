using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jannesen.FileFormat.Json
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class JsonObject: Dictionary<string, object>
    {
        public      static      JsonObject              Parse(JsonReader reader)
        {
            JsonObject      rtn = new JsonObject();

            if (reader.ReadChar() != (int)'{')
                throw new JsonReaderException("Expect '{'", reader);

            int c= reader.SkipWhiteSpace();

            while (c != (int)'}') {
                {
                    string name = (c == (int)'\'' || c == (int)'"') ? reader.ReadString() : reader.ReadChars();

                    c = reader.SkipWhiteSpace();
                    if (c != ':')
                        throw new JsonReaderException("Expect ':'", reader);

                    reader.ReadChar();

                    rtn.Add(name, reader.ParseNode());
                }

                {
                    c = reader.SkipWhiteSpace();

                    if (c == (int)',') {
                        reader.ReadChar();
                        c = reader.SkipWhiteSpace();
                    }
                    else
                    if (c != (int)'}')
                        throw new JsonReaderException("Expect ',' or ']'", reader);
                }
            }

            reader.ReadChar();

            return rtn;
        }

        public                                          JsonObject()
        {
        }
        public                                          JsonObject(int capacity): base(capacity)
        {
        }
        public                  object                  GetValue(string name)
        {
            if (!TryGetValue(name, out var rtn))
                throw new IndexOutOfRangeException("Unknown field '" + name + "' in JSON object.");

            return rtn;
        }
        public                  object                  GetValueNullable(string name)
        {
            TryGetValue(name, out var rtn);

            return rtn;
        }
        public                  string                  GetValueString(string name)
        {
            object v = GetValue(name);

            try {
                if (v is Int64) return ((Int64)v).ToString();
                return (string)v;
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  string                  GetValueStringNullable(string name)
        {
            object v = GetValueNullable(name);

            try {
                return (v == null) ? null : (string)v;
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  Int16                   GetValueInt16(string name)
        {
            object v = GetValue(name);

            try {
                return Convert.ToInt16(v);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  Int16?                  GetValueInt16Nullable(string name)
        {
            object v = GetValueNullable(name);

            try {
                return (v == null) ? (Int16?)v: Convert.ToInt16(v);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  int                     GetValueInt(string name)
        {
            object v = GetValue(name);

            try {
                return Convert.ToInt32(v);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  int?                    GetValueIntNullable(string name)
        {
            object v = GetValueNullable(name);

            try {
                return (v == null) ? (int?)v: Convert.ToInt32(v);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  DateTime                GetValueDateTime(string name)
        {
            string v = GetValueString(name);

            try {
                return ConvertStringToDateTime(v);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  DateTime?               GetValueDateTimeNullable(string name)
        {
            string v = GetValueStringNullable(name);

            if (v == null)
                return null;

            try {
                return ConvertStringToDateTime(v);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  JsonObject              GetValueObject(string name)
        {
            if (TryGetValue(name, out var v)) {
                if (v != null && !(v is JsonObject))
                    throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }

            return (JsonObject)v;
        }
        public                  JsonArray               GetValueArray(string name)
        {
            if (TryGetValue(name, out var v)) {
                if (v != null && !(v is JsonArray))
                    throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }

            return (JsonArray)v;
        }

        private     static      DateTime                ConvertStringToDateTime(string sValue)
        {
            int     fieldpos = 0;
            int[]   fields   = new int[7];
            int     factor   = 0;

            for (int pos = 0 ; pos<sValue.Length ; ++pos) {
                char    chr = sValue[pos];

                if (chr>='0' && chr <='9') {
                    if (fieldpos<6)
                        fields[fieldpos] = fields[fieldpos]*10 + (chr-'0');
                    else {
                        fields[fieldpos] += (chr-'0')*factor;
                        factor /= 10;
                    }
                }
                else {
                    switch(fieldpos) {
                    case 0: // year
                    case 1: // month
                        if (chr!='-') goto invalid_date;
                        break;

                    case 2: // day
                        if (chr!='T') goto invalid_date;
                        break;

                    case 3: // hour
                    case 4: // minute
                        if (chr!=':') goto invalid_date;
                        break;

                    case 5: // second
                        if (chr!='.') goto invalid_date;
                        factor = 100;
                        break;

                    default:
invalid_date:               throw new System.FormatException("Invalid date format.");
                    }

                    ++fieldpos;
                }
            }

            if (fields[1]<1 || fields[1]>12 ||
                fields[2]<1 || fields[2]>31 ||
                fields[3]>23 ||
                fields[4]>59 ||
                fields[5]>59)
                throw new FormatException("Invalid date format.");

            return new DateTime(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5], fields[6]);
        }
    }
}
