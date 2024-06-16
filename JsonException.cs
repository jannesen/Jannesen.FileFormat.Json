using System;

namespace Jannesen.FileFormat.Json
{
    public class JsonXmlReaderException: Exception
    {
        public                              JsonXmlReaderException(string message): base(message)
        {
        }
        public                              JsonXmlReaderException(string message, Exception innerException): base(message, innerException)
        {
        }

        public  override    string          Source      => "Jannesen.FileFormat.Json";
    }

    public class JsonReaderException: Exception
    {
        public                  int         LineNumber              { get ; }
        public                  int         LinePosition            { get ; }

        public                              JsonReaderException(string message, JsonReader reader): base(message)
        {
            ArgumentNullException.ThrowIfNull(reader);

            LineNumber   = reader.LineNumber;
            LinePosition = reader.LinePosition;
        }

        public  override    string          Source      => "Jannesen.FileFormat.Json";
    }
}
