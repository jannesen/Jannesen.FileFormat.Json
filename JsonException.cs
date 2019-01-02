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
            LineNumber   = reader.LineNumber;
            LinePosition = reader.LinePosition;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected                           JsonReaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.LineNumber   = info.GetInt32("LineNumber");
            this.LinePosition = info.GetInt32("LinePosition");
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public      override    void        GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("LineNumber",   this.LineNumber);
            info.AddValue("LinePosition", this.LinePosition);
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.FileFormat.Json";
            }
        }
    }
}
