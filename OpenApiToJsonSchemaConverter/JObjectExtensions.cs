using System.Linq;

using Newtonsoft.Json.Linq;

namespace OpenApiToJsonSchemaConverter
{
    public static class JObjectExtensions
    {
        public static JObject ConvertEnumsToInteger(this JObject schema)
        {
            foreach (var property in schema.Properties().ToList())
            {
                switch (property.Value)
                {
                    case JObject jObject:
                        ConvertEnumsToInteger(jObject);
                        ConvertEnumPropertyToInteger(jObject);
                        break;
                    case JArray jArray:
                        foreach (var item in jArray.OfType<JObject>())
                        {
                            ConvertEnumsToInteger(item);
                        }

                        break;
                }
            }

            return schema;
        }

        private static void ConvertEnumPropertyToInteger(this JObject jObject)
        {
            if (jObject.TryGetValue("enum", out _))
            {
                jObject.Remove("enum");

                if (jObject.TryGetValue("type", out var type))
                {
                    switch (type.Type)
                    {
                        case JTokenType.String:
                            var typeValue = type.Value<string>();
                            if (typeValue != null && typeValue != "integer")
                            {
                                jObject["type"] = new JArray(typeValue, "integer");
                            }

                            break;
                        case JTokenType.Array when !type.Values<string>().Contains("integer"):
                            ((JArray)type).Add("integer");
                            break;
                    }
                }
            }
        }

        public static JObject MakeAllPropertiesNullable(this JObject schema)
        {
            foreach (var property in schema.Properties().ToList())
            {
                switch (property.Value)
                {
                    case JObject jObject:
                        MakeAllPropertiesNullable(jObject);
                        MakePropertyNullable(jObject);
                        break;
                    case JArray jArray:
                        foreach (var item in jArray.OfType<JObject>())
                        {
                            MakeAllPropertiesNullable(item);
                        }

                        break;
                }
            }

            return schema;
        }

        private static void MakePropertyNullable(this JObject jObject)
        {
            if (jObject.TryGetValue("type", out var type))
            {
                switch (type.Type)
                {
                    case JTokenType.String:
                        var typeValue = type.Value<string>();
                        if (typeValue != null && typeValue != "null")
                        {
                            jObject["type"] = new JArray(typeValue, "null");
                        }

                        break;
                    case JTokenType.Array when !type.Values<string>().Contains("null"):
                        ((JArray)type).Add("null");
                        break;
                }
            }
        }

        public static JObject AddIntegerTypeIfString(this JObject schema)
        {
            foreach (var property in schema.Properties().ToList())
            {
                if (property.Value is JObject jObject)
                {
                    AddIntegerTypeIfString(jObject);
                    AddIntegerTypeIfStringProperty(jObject);
                }
                else if (property.Value is JArray jArray)
                {
                    foreach (var item in jArray.OfType<JObject>())
                    {
                        AddIntegerTypeIfString(item);
                    }
                }
            }

            return schema;
        }

        private static void AddIntegerTypeIfStringProperty(this JObject jObject)
        {
            if (jObject.ContainsKey("type"))
            {
                var type = jObject["type"];
                if (type != null)
                {
                    switch (type.Type)
                    {
                        case JTokenType.String:
                            var typeValue = type.Value<string>();
                            if (typeValue is "string")
                            {
                                jObject["type"] = new JArray(typeValue, "integer");
                            }

                            break;
                        case JTokenType.Array:
                            var typeValues = type.Values<string>();
                            if (typeValues.Contains("string") && !typeValues.Contains("integer"))
                            {
                                ((JArray)type).Add("integer");
                            }

                            break;
                    }
                }
            }
        }
    }
}
