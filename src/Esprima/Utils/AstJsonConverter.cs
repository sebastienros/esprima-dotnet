#if NET5_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Esprima.Ast;

namespace Esprima.Utils
{
    public class AstJsonConverter : JsonConverter<object>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(Node).IsAssignableFrom(typeToConvert);
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var propertyBag = new Dictionary<string, object>();
            reader.Read();
            while (true)
            {
                if (reader.TokenType != JsonTokenType.PropertyName && reader.TokenType != JsonTokenType.String)
                {
                    break;
                }

                var name = reader.GetString();
                reader.Read();

                object value = null;
                if (reader.TokenType == JsonTokenType.String)
                {
                    value = reader.GetString();
                    reader.Read();
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    value = reader.GetDouble();
                    reader.Read();
                }
                else if (reader.TokenType == JsonTokenType.True)
                {
                    value = true;
                    reader.Read();
                }
                else if (reader.TokenType == JsonTokenType.False)
                {
                    value = false;
                    reader.Read();
                }
                else if (reader.TokenType == JsonTokenType.Null)
                {
                    value = null;
                    reader.Read();
                }
                else if (reader.TokenType == JsonTokenType.StartArray)
                {
                    var values = new List<Node>();
                    reader.Read();
                    while (true)
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                            break;
                        var v = (Node) JsonSerializer.Deserialize(ref reader, typeof(Node), options);
                        reader.Read();
                        values.Add(v);
                    }
                    reader.Read();
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    value = JsonSerializer.Deserialize(ref reader, typeof(Node), options);
                    reader.Read();
                }
                else
                {
                    reader.Read();
                }
                propertyBag.Add(name, value);
            }

            var type = (string)propertyBag["type"];
            switch (type)
            {
                case "AssignmentExpression":
                    return new AssignmentExpression((string) propertyBag["op"], (Expression) propertyBag["left"], (Expression) propertyBag["right"]);
                case "ArrayExpression":
                    return new ArrayExpression(NodeList.Create(((List<Node>) propertyBag["elements"]).Cast<Expression>()));
                case "BlockStatement":
                    return new BlockStatement(NodeList.Create(((List<Node>) propertyBag["elements"]).Cast<Statement>()));
                case "BinaryExpression":
                    return new BinaryExpression((string) propertyBag["op"], (Expression) propertyBag["left"], (Expression) propertyBag["right"]);
                case "Identifier":
                    return new Identifier((string) propertyBag["name"]);
                case "Literal":
                    {
                        var value = propertyBag["value"];
                        if (value is bool)
                            return new Literal((bool) value, (string) propertyBag["raw"]);
                        if (value is double)
                            return new Literal((double) value, (string) propertyBag["raw"]);
                        return new Literal((string) value, (string) propertyBag["raw"]);
                    }
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

       
    }
}
#endif
