using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jannesen.FileFormat.Json
{
    [Serializable]
    public class JsonArray: List<object>
    {
        public                                          JsonArray()
        {
        }
        public                                          JsonArray(int capacity): base(capacity)
        {
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
