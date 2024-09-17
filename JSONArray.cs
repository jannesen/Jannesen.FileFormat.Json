using System;
using System.Collections.Generic;

namespace Jannesen.FileFormat.Json
{
    public class JsonArray: List<object?>, IJsonSerializer
    {
        public                                          JsonArray()
        {
        }
        public                                          JsonArray(int capacity): base(capacity)
        {
        }

        public                  void                    WriteTo(JsonWriter writer)
        {
            writer.WriteArray(this);
        }

        public  override        bool                    Equals(object? obj)
        {
            if (obj is JsonArray other) {
                if (this.Count != other.Count) {
                    return false;
                }

                for (var i = 0 ; i < this.Count ; ++i) {
                    if (!object.Equals(this[i], other[i])) {
                        return false;
                    }                    
                }

                return true;
            }

            return false;
        }
        public  override        int                     GetHashCode()
        {
            var rtn = 0;

            foreach(var e in this) {
                if (e != null ) {
                    rtn ^= e.GetHashCode();
                }                    
            }

            return rtn;
        }

        public      static      JsonArray               Parse(JsonReader reader)
        {
            ArgumentNullException.ThrowIfNull(reader);

            var rtn = new JsonArray();

            if (reader.ReadChar() !=(int)'[')
                throw new JsonReaderException("Expect '['", reader);

            var c= reader.SkipWhiteSpace();

            while (c != (int)']') {
                {
                    rtn.Add(reader.ParseNode());
                }

                {
                    c = reader.SkipWhiteSpace();

                    if (c == (int)',') {
                        reader.ReadChar();
                        c = reader.SkipWhiteSpace();
                    }
                    else
                    if (c != (int)']')
                        throw new JsonReaderException("Expect ',' or ']'", reader);
                }
            }

            reader.ReadChar();

            return rtn;
        }
    }
}
