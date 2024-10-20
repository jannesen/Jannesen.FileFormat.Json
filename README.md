# Jannesen.FileFormat.Json

Basic Json reader/writer.

## Translation

| json type  | C# type                                        |
|:-----------|:-----------------------------------------------|
| string     | System.string                                  |
| double     | System.double                                  |
| int        | System.Int64                                   |
| boolean    | system.Boolean                                 |
| null       | null                                           |
| object     | JsonObject (Dictionary&lt;string, object&gt;)  |
| array      | JsonArray  (List&lt;object&gt;)                |


## classes

| name          | description |
|:--------------|:-----------------------------------------------|
| JsonReader    | Read a json data from a TextReader and convert it to a json document using JsonObject,JsonArray,string,Int64,double,boolean.
| JsonWriter    | Write json document to a TextWriter or use method to construct Json like XmlWriter
| JsonObject    | Json object
| JsonArray     | Json array
| JsonXmlReader | translate a xml document to json document
