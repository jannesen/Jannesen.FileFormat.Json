﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Jannesen.FileFormat.Json
{
    [Serializable]
    public class JsonObject: Dictionary<string, object>
    {
        public      static      JsonObject              Parse(JsonReader reader)
        {
            if (reader is null) throw new ArgumentNullException(nameof(reader));

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
        protected                                       JsonObject(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }

        public                  object                  GetValue(string name)
        {
            if (!TryGetValue(name, out var rtn))
                throw new IndexOutOfRangeException("Unknown field '" + name + "' in JSON object.");

            if (rtn == null) { 
                throw new FormatException("Field '" + name + "' = null.");
            }

            return rtn;
        }
        public                  object                  GetValueNullable(string name)
        {
            TryGetValue(name, out var rtn);

            return rtn;
        }
        public                  string                  GetValueString(string name)
        {
            var v = GetValue(name);

            try {
                return _convertToString(v);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  string                  GetValueStringNullable(string name)
        {
            var v = GetValueNullable(name);

            if (v == null)
                return null;

            try {
                return _convertToString(v);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  bool                    GetValueBoolean(string name)
        {
            var v = GetValue(name);

            try {
                return _convertToBoolean(v);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  bool?                   GetValueBooleanNullable(string name)
        {
            var v = GetValueNullable(name);

            if (v == null)
                return null;

            try {
                return _convertToBoolean(v);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  Int16                   GetValueInt16(string name)
        {
            var v = GetValue(name);

            try {
                return Convert.ToInt16(v, CultureInfo.InvariantCulture);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  Int16?                  GetValueInt16Nullable(string name)
        {
            var v = GetValueNullable(name);

            if (v == null)
                return null;

            try {
                return Convert.ToInt16(v, CultureInfo.InvariantCulture);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  int                     GetValueInt(string name)
        {
            var v = GetValue(name);

            try {
                return Convert.ToInt32(v, CultureInfo.InvariantCulture);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  int?                    GetValueIntNullable(string name)
        {
            var v = GetValueNullable(name);

            if (v == null)
                return null;

            try {
                return Convert.ToInt32(v, CultureInfo.InvariantCulture);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  Decimal                 GetValueDecimal(string name)
        {
            var v = GetValue(name);

            try {
                return Convert.ToDecimal(v, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  Decimal?                GetValueDecimalNullable(string name)
        {
            var v = GetValueNullable(name);

            if (v == null)
                return null;

            try {
                return Convert.ToDecimal(v, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  double                  GetValueDouble(string name)
        {
            var v = GetValue(name);

            try {
                return Convert.ToDouble(v, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  double?                 GetValueDoubleNullable(string name)
        {
            var v = GetValueNullable(name);

            if (v == null)
                return null;

            try {
                return Convert.ToDouble(v, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  DateTime                GetValueDateTime(string name)
        {
            var v = GetValueString(name);

            try {
                return _convertToDateTime(v);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  DateTime?               GetValueDateTimeNullable(string name)
        {
            var v = GetValueStringNullable(name);

            if (v == null)
                return null;

            try {
                return _convertToDateTime(v);
            }
            catch(Exception) {
                throw new FormatException("Invalid value JSON object field '" + name + "'.");
            }
        }
        public                  JsonObject              GetValueObject(string name)
        {
            if (TryGetValue(name, out var v)) {
                if (v != null) {
                    if (v is JsonObject o)
                        return o;

                    throw new FormatException("Invalid value JSON object field '" + name + "'.");
                }
            }

            return null;
        }
        public                  JsonArray               GetValueArray(string name)
        {
            if (TryGetValue(name, out var v)) {
                if (v != null) {
                    if (v is JsonArray a)
                        return a;

                    throw new FormatException("Invalid value JSON array field '" + name + "'.");
                }
            }

            return null;
        }

        private     static      string                  _convertToString(object v)
        {
            if (v is string s)  return s;
            if (v is Int64  i)  return i.ToString(CultureInfo.InvariantCulture);

            throw new FormatException("Invalid string value");
        }
        private     static      bool                    _convertToBoolean(object v)
        {
            if (v is bool b)    return b;
            if (v is Int64 i) {
                if (i == 0) return false;
                if (i == 1) return true;
            }

            throw new FormatException("Invalid boolean value");
        }
        private     static      DateTime                _convertToDateTime(string sValue)
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
