using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Jannesen.FileFormat.Json
{
    public sealed class JsonReader: IDisposable
    {
        private readonly        TextReader              _textReader;
        private readonly        bool                    _keepopen;
        private                 int                     _lineNumber;
        private                 int                     _linePosition;
        private                 int                     _undoChar;

        public                  int                     LineNumber      => _lineNumber;
        public                  int                     LinePosition    => _linePosition;

        public                                          JsonReader(TextReader textReader, bool keepopen=false)
        {
            _textReader   = textReader;
            _keepopen     = keepopen;
            _lineNumber   = 1;
            _linePosition = 0;
            _undoChar     = -1;
        }
        public                  void                    Dispose()
        {
            if (!_keepopen) {
                _textReader.Dispose();
            }
        }

        public      static      object?                 ParseString(string s)
        {
            using (var reader = new JsonReader(new StringReader(s))) {
                return reader.ParseDocument();
            }
        }
        public      static      object?                 ParseFile(string fileName)
        {
            using(var reader = new JsonReader(new StreamReader(fileName))) {
                return reader.ParseDocument();
            }
        }
        public                  object?                 ParseDocument()
        {
            var n = ParseNode();
            //test eof
            return n;
        }
        public                  object?                 ParseNode()
        {
            var c = SkipWhiteSpace();

            switch(c) {
            case  (int)'[':     return JsonArray.Parse(this);
            case (int)'{':      return JsonObject.Parse(this);
            case (int)'"':      return ReadString();

            default:
                {
                    var s = ReadChars();

                    if (s == "null")    return null;
                    if (s == "true")    return true;
                    if (s == "false")   return false;

                    var p = 0;

                    if (s[0] == '+' || s[0] == '-')
                        ++p;

                    while (p < s.Length && ('0' <= s[p] && s[p] <= '9'))
                        ++p;

                    if (p == s.Length) {
                        if (Int64.TryParse(s, out var rtn))
                            return rtn;
                    }
                    else {
                        if (s[p] == '.' || s[p] == 'e' || s[p] == 'E') {
                            if (s[p] == '.') {
                                ++p;

                                while (p < s.Length && ('0' <= s[p] && s[p] <= '9'))
                                    ++p;
                            }

                            if (p < s.Length && (s[p] == 'e' || s[p] == 'E')) {
                                ++p;

                                if (p < s.Length && (s[p] == '+' || s[p] == '-'))
                                    ++p;

                                while (p < s.Length && ('0' <= s[p] && s[p] <= '9'))
                                    ++p;
                            }
                        }

                        if (p == s.Length) {
                            if (double.TryParse(s,
                                                NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowLeadingSign,
                                                CultureInfo.InvariantCulture,
                                                out var rtn))
                                return rtn;
                        }
                    }

                    throw new JsonReaderException("Invalid value", this);
                }
            }
        }

        public                  int                     SkipWhiteSpace()
        {
            int     c;

            while (_isWhiteSpace(c = ReadChar()))
                ;

            _undoChar = c;

            return c;
        }
        public                  int                     ReadChar()
        {
            int  c;

            if (_undoChar >= 0) {
                c = _undoChar;
                _undoChar = -1;
            }
            else {
                c = _textReader.Read();

                if (c == -1)
                    throw new JsonReaderException("EOF read", this);

                if (c == '\n') {
                    ++_lineNumber;
                    _linePosition = 0;
                }
                else
                    ++_linePosition;
            }

            return c;
        }
        public                  string                  ReadString()
        {
            var e   = ReadChar();
            var rtn = new StringBuilder();
            int c;

            while ((c = ReadChar()) != e) {
                if (c == (int)'\\') {
                    c = ReadChar();

                    switch(c) {
                    case 'b':       c = (int)'\b';      break;
                    case 'f':       c = (int)'\f';      break;
                    case 'n':       c = (int)'\n';      break;
                    case 'r':       c = (int)'\r';      break;
                    case 't':       c = (int)'\t';      break;
                    case 'u':       c = (ReadCharHex() << 12) | (ReadCharHex() << 8) | (ReadCharHex() << 4) | (ReadCharHex() << 0);        break;
                    }
                }

                rtn.Append((char)c);
            }

            return rtn.ToString();
        }
        public                  string                  ReadChars()
        {
            var rtn = new StringBuilder();
            int c;

            while (_validChars(c = ReadChar()))
                rtn.Append((char)c);

            _undoChar = c;

            if (rtn.Length == 0)
                throw new JsonReaderException("Invalid character", this);

            return rtn.ToString();
        }
        public                  int                     ReadCharHex()
        {
            var c = ReadChar();

            if ((int)'0' <= c && c <= (int)'9')     return c - (int)'0';
            if ((int)'A' <= c && c <= (int)'F')     return c - (int)'A' + 10;
            if ((int)'a' <= c && c <= (int)'f')     return c - (int)'a' + 10;

            throw new JsonReaderException("Invalid hexadecimal digit.", this);
        }

        private     static      bool                    _isWhiteSpace(int c)
        {
            return c == (int)' ' || c == (int)'\t' || c == (int)'\n' || c == (int)'\r';
        }
        private     static      bool                    _validChars(int c)
        {
            return ((int)'0' <= c && c <= (int)'9') ||
                   ((int)'A' <= c && c <= (int)'Z') ||
                   ((int)'a' <= c && c <= (int)'z') ||
                   (c == (int)'$' || c == (int)'_' || c == (int)'-' || c == (int)'+' || c == (int)'.');
        }
    }
}
