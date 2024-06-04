using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace OpenApiToJsonSchemaConverter
{
    public static class OpenApiSchemaExtensions
    {
        public static List<JSchema> ConvertOpenApiSchemaToJsonSchema(this OpenApiDocument apiDocument)
        {
            var componentSchemas = apiDocument.ParseComponentSchemas();
            return apiDocument.Components.Schemas.Select(t => t.Value).Select(schema => schema.ToJsonSchema(componentSchemas)).ToList();
        }

        public static JSchema ToJsonSchema(this OpenApiSchema openApiSchema, Dictionary<string, Dictionary<string, JObject>> componentsSchemas = null)
        {
            OpenApiSchemaToJsonSchema.AllowNullsGlobally = true;
            OpenApiSchemaToJsonSchema.CSharpIntToEnumConversion = true;
            OpenApiSchemaToJsonSchema.IntegerValuesOnStringFields = true;

            var schemaDict = OpenApiSchemaToJsonSchema.ConvertToJSONSchema(openApiSchema);

            // JsonPatchDocument causes an error when converting to a JSchema
            if (schemaDict.ContainsKey("items"))
            {
                schemaDict.Remove("items");
            }

            // Add the components dictionary to the main schema dictionary
            schemaDict.Add("components", componentsSchemas);

            // Convert the updated dictionary back to a string
            var schemaString = JsonConvert.SerializeObject(schemaDict);

            // Parse the string to a JSchema
            var schema = JSchema.Parse(schemaString);

            return schema;
        }

        public static Dictionary<string, Dictionary<string, JObject>> ParseComponentSchemas(
            this OpenApiDocument apiDocument)
        {
            var componentsDict = new Dictionary<string, Dictionary<string, JObject>>();
            var schemasDict = new Dictionary<string, JObject>();

            OpenApiSchemaToJsonSchema.AllowNullsGlobally = true;
            OpenApiSchemaToJsonSchema.CSharpIntToEnumConversion = true;
            OpenApiSchemaToJsonSchema.IntegerValuesOnStringFields = true;

            // Convert each component schema and add it to the components dictionary
            foreach (var component in apiDocument.Components.Schemas)
            {
                var componentSchemaDict = OpenApiSchemaToJsonSchema.ConvertToJSONSchema(component.Value);

                schemasDict.Add(component.Key, JObject.FromObject(componentSchemaDict));
            }

            componentsDict.Add("schemas", schemasDict);
            return componentsDict;
        }
    }
}
