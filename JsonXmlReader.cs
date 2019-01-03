using System;
using System.Xml;
using System.IO;

namespace Jannesen.FileFormat.Json
{
    public class JsonXmlReader
    {
        public      static      object              ParseXmlString(string xmlString)
        {
            using (XmlTextReader xmlReader = new XmlTextReader(new StringReader(xmlString)))
                return Parse(xmlReader);
        }
        public      static      object              Parse(XmlTextReader xmlReader)
        {
            while (xmlReader.NodeType != XmlNodeType.Element)
                _parseReadNode(xmlReader);

            if (xmlReader.Name.Length >= 3 && xmlReader.Name[0] == '_' && xmlReader.Name[2] == '_') {
                switch(xmlReader.Name[1]) {
                case 'a':       return _parseToJsonArray(xmlReader);
                case 'o':       return _parseToJsonObject(xmlReader);
                default:        return _jsonConvertValue(xmlReader.Name[1], _parseElementValue(xmlReader));
                }
            }

            if (xmlReader.Name=="json-array")
                return _parseToJsonArray(xmlReader);

            return _parseToJsonObject(xmlReader);
        }

        private     static      JsonObject          _parseToJsonObject(XmlTextReader xmlReader)
        {
            JsonObject  jsonObject = new JsonObject();

            if (xmlReader.MoveToFirstAttribute()) {
                do {
                    if (string.IsNullOrEmpty(xmlReader.NamespaceURI)) {
                        string name   = xmlReader.Name;
                        string svalue = xmlReader.Value;

                        if (name.Length > 3 && name[0] == '_' && name[2] == '_') {
                            try {
                                jsonObject.Add(name.Substring(3), _jsonConvertValue(name[1], svalue));
                            }
                            catch(Exception err) {
                                throw new JsonXmlReaderException("Invalid attribute value '" + name + "' value='" + svalue + "'.", err);
                            }
                        }
                        else
                            jsonObject.Add(name, svalue);
                    }
                }
                while (xmlReader.MoveToNextAttribute());

                xmlReader.MoveToElement();
            }

            if (xmlReader.IsEmptyElement)
                return jsonObject;

            for (;;) {
                _parseReadNode(xmlReader);

                switch(xmlReader.NodeType) {
                case XmlNodeType.EndElement:
                    return jsonObject;

                case XmlNodeType.Element:
                    {
                        string  name = xmlReader.Name;

                        try {
                            if (name.Length > 3 && name[0] == '_' && name[2] == '_') {
                                switch(name[1]) {
                                case 'a':       jsonObject.Add(name.Substring(3), _parseToJsonArray(xmlReader));                                    break;
                                case 'o':       jsonObject.Add(name.Substring(3), _parseToJsonObject(xmlReader));                                   break;
                                default:        jsonObject.Add(name.Substring(3), _jsonConvertValue(name[1], _parseElementValue(xmlReader)));       break;
                                }
                            }
                            else {
                                if (jsonObject.TryGetValue(name, out var obj)) {
                                    if (!(obj is JsonArray))
                                        throw new JsonXmlReaderException("Variable already defined as a non-array.");
                                }
                                else
                                    jsonObject.Add(xmlReader.Name, obj = new JsonArray());

                                ((JsonArray)obj).Add(_parseToJsonObject(xmlReader));
                            }
                        }
                        catch(Exception err) {
                            throw new JsonXmlReaderException("Conversie failed in element '" + name + "'.", err);
                        }
                    }
                    break;
                }
            }
        }
        private     static      JsonArray           _parseToJsonArray(XmlTextReader xmlReader)
        {
            JsonArray   rtn = new JsonArray();

            if (!xmlReader.IsEmptyElement) {
                for (;;) {
                    _parseReadNode(xmlReader);

                    switch(xmlReader.NodeType) {
                    case XmlNodeType.EndElement:
                        return rtn;

                    case XmlNodeType.Element:
                        if (xmlReader.Name != "row")
                            throw new JsonXmlReaderException("Expect 'row' element.");

                        rtn.Add(_parseToJsonObject(xmlReader));
                        break;
                    }
                }
            }

            return rtn;
        }
        private     static      object              _jsonConvertValue(char t, string svalue)
        {
            switch(t) {
            case 's':   return svalue;
            case 'i':   return !string.IsNullOrEmpty(svalue) ? (object)int.Parse(svalue, System.Globalization.CultureInfo.InvariantCulture)   : null;
            case 'n':   return !string.IsNullOrEmpty(svalue) ? (object)double.Parse(svalue, System.Globalization.CultureInfo.InvariantCulture) : null;

            case 'b':
                switch(svalue) {
                case null:      return null;
                case "":        return null;
                case "0":       return false;
                case "1":       return true;
                default:        throw new JsonXmlReaderException("Invalid boolean value '" + svalue + "'.");
                }

            default:
                throw new JsonXmlReaderException("Invalid json-typeinfo in '" + t + "'.");
            }
        }
        private     static      string              _parseElementValue(XmlTextReader xmlReader)
        {
            if (!xmlReader.IsEmptyElement) {
                string  rtn = "";

                for (;;) {
                    _parseReadNode(xmlReader);

                    switch(xmlReader.NodeType) {
                    case XmlNodeType.EndElement:
                        return rtn;

                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        rtn += xmlReader.Value;
                        break;

                    default:
                        throw new JsonXmlReaderException("Value element has a child element ('" + xmlReader.NodeType.ToString() + "').");
                    }
                }
            }
            else {
                if (xmlReader.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance") == "true")
                    return null;

                return "";
            }
        }
        private     static      void                _parseReadNode(XmlTextReader xmlReader)
        {
            if (!xmlReader.Read())
                throw new JsonXmlReaderException("Reading EOF on xml-document.");
        }
    }
}
