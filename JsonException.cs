using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Jannesen.FileFormat.Json
{
    [Serializable]
    public class JsonXmlReaderException: Exception
    {
        public                              JsonXmlReaderException(string message): base(message)
        {
        }
        public                              JsonXmlReaderException(string message, Exception innerException): base(message, innerException)
        {
        }
        protected                           JsonXmlReaderException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.FileFormat.Json";
            }
        }
    }

    [Serializable]
    public class JsonReaderException: Exception
    {
        public                  int         LineNumber              { get ; }
        public                  int         LinePosition            { get ; }

        public                              JsonReaderException(string message, JsonReader reader): base(message)
        {
            if (reader is null) throw new ArgumentNullException(nameof(reader));

            LineNumber   = reader.LineNumber;
            LinePosition = reader.LinePosition;
        }

        protected                           JsonReaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.LineNumber   = info.GetInt32(nameof(LineNumber));
            this.LinePosition = info.GetInt32(nameof(LinePosition));
        }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public      override    void        GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(LineNumber),   this.LineNumber);
            info.AddValue(nameof(LinePosition), this.LinePosition);
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.FileFormat.Json";
            }
        }
    }
}
