using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jannesen.FileFormat.Json
{
    public class JsonArray: List<object>, IJsonSerializer
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

        public  override        bool                    Equals(object obj)
        {
            if (obj is JsonArray other) {
                if (this.Count != other.Count) {
                    return false;
                }

                for (int i = 0 ; i < this.Count ; ++i) {
                    if (this[i] != null ? !this[i].Equals(other[i]) : other[i] == null) {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
        public  override        int                     GetHashCode()
        {
            int rtn = 0;

            foreach(var e in this) {
                if (e != null ) {
                    rtn ^= e.GetHashCode();
                }
            }

            return rtn;
        }

        public      static      JsonArray               Parse(JsonReader reader)
        {
            if (reader is null) throw new ArgumentNullException(nameof(reader));

            JsonArray       rtn = new JsonArray();

            if (reader.ReadChar() !=(int)'[')
                throw new JsonReaderException("Expect '['", reader);

            int c= reader.SkipWhiteSpace();

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
