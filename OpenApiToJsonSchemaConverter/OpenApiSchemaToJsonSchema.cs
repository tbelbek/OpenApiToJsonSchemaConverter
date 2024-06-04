using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace OpenApiToJsonSchemaConverter
{
    internal class OpenApiSchemaToJsonSchema
    {
        private static readonly List<string> _notSupported = new List<string>
                                                                 {
                                                                     "nullable", "discriminator", "readOnly", "writeOnly", "xml", "externalDocs", "example", "deprecated"
                                                                 };

        // Global override for allowing nulls on converted json schema properties
        public static bool AllowNullsGlobally { get; set; } = false;

        // Accept CSharp style integer enums
        public static bool CSharpIntToEnumConversion { get; set; } = false;

        // Accept integer values on string fields
        public static bool IntegerValuesOnStringFields { get; set; } = false;

        /// <summary>
        /// Prepares the schema and options for conversion.
        /// </summary>
        /// <param name="inputSchema">The schema to be prepared for conversion.</param>
        /// <param name="conversionOptions">The options for the conversion process.</param>
        /// <returns>A tuple containing the prepared schema and the options for conversion.</returns>
        public static (Dictionary<string, object> PreparedSchema, OpenApiToJsonSchemaConverterOptions ConversionOptions) PrepareForConversion(Dictionary<string, object> inputSchema, OpenApiToJsonSchemaConverterOptions conversionOptions = null)
        {
            // If options are not provided, initialize with default options
            conversionOptions = conversionOptions ?? new OpenApiToJsonSchemaConverterOptions();

            // Resolve not supported properties
            conversionOptions.NotSupported = ResolveNotSupported(_notSupported, conversionOptions.KeepNotSupported);

            // If RemoveReadOnly is set, add "readOnly" to the list of properties to remove
            if (conversionOptions.RemoveReadOnly)
            {
                conversionOptions.RemoveProps.Add("readOnly");
            }

            // If RemoveWriteOnly is set, add "writeOnly" to the list of properties to remove
            if (conversionOptions.RemoveWriteOnly)
            {
                conversionOptions.RemoveProps.Add("writeOnly");
            }

            // If CloneSchema is set, clone the schema
            if (conversionOptions.CloneSchema)
            {
                // Clone the schema by serializing and then deserializing it
                inputSchema = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(inputSchema));
            }

            // Return the prepared schema and options
            return (inputSchema, conversionOptions);
        }

        /// <summary>
        /// Converts an OpenApiSchema to a JSON schema.
        /// </summary>
        /// <param name="openApiInputSchema">The OpenApiSchema to be converted.</param>
        /// <param name="conversionOptions">The options for the conversion process.</param>
        /// <returns>A dictionary representing the JSON schema.</returns>
        /// <exception cref="OpenApiSchemaConversionException">Thrown when the schema is null after preparation.</exception>
        public static Dictionary<string, object> ConvertToJSONSchema(OpenApiSchema openApiInputSchema, OpenApiToJsonSchemaConverterOptions conversionOptions = null)
        {
            // Convert the OpenApiSchema to a string
            using (var stringWriter = new StringWriter())
            {
                var openApiWriter = new OpenApiJsonWriter(stringWriter);
                openApiInputSchema.SerializeAsV3(openApiWriter);
                var openApiString = stringWriter.ToString(); // The OpenApiSchema is now a string

                // Convert the string to a Dictionary<string, object>
                var schemaDictionary =
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        openApiString); // The string is now a dictionary

                // Prepare the schema and options
                (schemaDictionary, conversionOptions) =
                    PrepareForConversion(
                        schemaDictionary,
                        conversionOptions); // The schema and options are now prepared for conversion

                // Convert the schema
                if (schemaDictionary != null)
                {
                    schemaDictionary = ConvertSchema(schemaDictionary, conversionOptions); // The schema is now converted

                    // Add the JSON schema version
                    schemaDictionary["$schema"] =
                        "http://json-schema.org/draft-04/schema#"; // The JSON schema version is added

                    return schemaDictionary; // The converted schema is returned
                }

                throw new OpenApiSchemaConversionException(); // An exception is thrown if the schema is null
            }
        }

        /// <summary>
        /// Recursively processes a tree structure, converting schemas and processing sub-trees and lists.
        /// </summary>
        /// <param name="tree">The tree to be processed.</param>
        /// <param name="options">The options for the conversion process.</param>
        /// <returns>A dictionary representing the processed tree.</returns>
        public static Dictionary<string, object> ProcessTree(Dictionary<string, object> tree, OpenApiToJsonSchemaConverterOptions options)
        {
            var keys = new List<string>(tree.Keys);
            foreach (var key in keys)
            {
                // Process each key-value pair in the tree
                switch (tree[key])
                {
                    // If the value is a schema, convert it
                    case Dictionary<string, object> subtree when key == "schema":
                        tree[key] = ConvertSchema(subtree, options);
                        break;
                    // If the value is a sub-tree, process it
                    case Dictionary<string, object> subtree:
                        tree[key] = ProcessTree(subtree, options);
                        break;
                    // If the value is a list, process each item in the list
                    case List<object> list:
                        tree[key] = list.Select(
                                item => item is Dictionary<string, object> dict ? ProcessTree(dict, options) : item)
                            .ToList();
                        break;
                    default:
                        // If the value is anything else, leave it as is
                        tree[key] = tree[key];
                        break;
                }
            }

            // Return the processed tree
            return tree;
        }

        /// <summary>
        /// Converts a document by preparing it for conversion, converting schemas, processing paths, and adding the JSON schema version.
        /// </summary>
        /// <param name="doc">The document to be converted.</param>
        /// <param name="options">The options for the conversion process.</param>
        /// <returns>A dictionary representing the converted document, or null if the document is null.</returns>
        public static Dictionary<string, object> ConvertDocument(Dictionary<string, object> doc, OpenApiToJsonSchemaConverterOptions options = null)
        {
            // Prepare the document and options for conversion
            (doc, options) = PrepareForConversion(doc, options);

            // If the document contains components with schemas, convert each schema
            if (doc?.ContainsKey("components") == true && doc["components"] is Dictionary<string, object> components && components?.ContainsKey("schemas") == true && components["schemas"] is Dictionary<string, object> schemas)
            {
                foreach (var name in schemas.Keys.ToList())
                {
                    schemas[name] = ConvertSchema((Dictionary<string, object>)schemas[name], options);
                }
            }

            // If the document contains paths, process each path
            if (doc?.ContainsKey("paths") == true && doc["paths"] is Dictionary<string, object> paths)
            {
                doc["paths"] = paths.ToDictionary(pair => pair.Key, pair => ProcessTree((Dictionary<string, object>)pair.Value, options));
            }

            // If the document is not null, add the JSON schema version
            if (doc != null)
            {
                doc["$schema"] = "http://json-schema.org/draft-04/schema#";
            }

            // Return the converted document
            return doc;
        }

        /// <summary>
        /// Converts a schema by processing structures, converting properties, validating and converting types, converting pattern properties, and removing unsupported properties.
        /// </summary>
        /// <param name="inputSchema">The schema to be converted.</param>
        /// <param name="conversionOptions">The options for the conversion process.</param>
        /// <returns>A dictionary representing the converted schema.</returns>
        public static Dictionary<string, object> ConvertSchema(Dictionary<string, object> inputSchema, OpenApiToJsonSchemaConverterOptions conversionOptions)
        {
            // Extract the structures and unsupported properties from the options
            var structures = conversionOptions?.Structs;
            var unsupportedProperties = conversionOptions?.NotSupported;

            // If there are structures, process each one
            if (structures != null)
            {
                foreach (var structureKey in structures.Where(structureKey => inputSchema.ContainsKey(structureKey)))
                {
                    // If the structure is a list, convert each item
                    // If the structure is a dictionary, convert it
                    switch (inputSchema[structureKey])
                    {
                        case List<object> structureList:
                            {
                                for (var i = 0; i < structureList.Count; i++)
                                {
                                    if (structureList[i] is Dictionary<string, object> item)
                                    {
                                        structureList[i] = ConvertSchema(item, conversionOptions);
                                    }
                                }

                                break;
                            }

                        case Dictionary<string, object> structureDict:
                            inputSchema[structureKey] = ConvertSchema(structureDict, conversionOptions);
                            break;
                    }
                }
            }

            var stringProperties = inputSchema.TryGetValue("properties", out var value) ? JsonConvert.SerializeObject(value) : null;

            // If the schema has properties, convert them
            if (inputSchema.ContainsKey("properties") && inputSchema["properties"] is Dictionary<string, object> properties)
            {
                inputSchema["properties"] = ConvertProperties(properties, conversionOptions);

                // If the schema has required properties, clean them
                if (inputSchema.ContainsKey("required") && inputSchema["required"] is List<string> required)
                {
                    inputSchema["required"] = CleanRequired(required, properties);

                    // If there are no required properties, remove the "required" key
                    if (((List<object>)inputSchema["required"]).Count == 0)
                    {
                        inputSchema.Remove("required");
                    }
                }

                // If there are no properties, remove the "properties" key
                if (properties.Count == 0)
                {
                    inputSchema.Remove("properties");
                }
            }

            if (inputSchema.ContainsKey("properties") && inputSchema["properties"] is JObject)
            {
                // Rule Exception: Allow integer enum values on request bodies
                if (CSharpIntToEnumConversion)
                {
                    inputSchema["properties"] = ((JObject)inputSchema["properties"]).ConvertEnumsToInteger();
                }

                // Rule Exception: If the addDefaultNullable flag is set, set all properties to nullable
                if (AllowNullsGlobally)
                {
                    inputSchema["properties"] = ((JObject)inputSchema["properties"]).MakeAllPropertiesNullable();
                }

                // Rule Exception: Allow string fields to have integer values
                if (IntegerValuesOnStringFields)
                {
                    inputSchema["properties"] = ((JObject)inputSchema["properties"]).AddIntegerTypeIfString();
                }
            }

            // Validate the type of the schema
            ValidateType(inputSchema.ContainsKey("type") ? inputSchema["type"]?.ToString() : null);

            // Convert the types in the schema
            inputSchema = ConvertTypes(inputSchema, conversionOptions);

            // If the schema has pattern properties and pattern properties are supported, convert them
            if (conversionOptions != null && inputSchema.ContainsKey("x-patternProperties") && inputSchema["x-patternProperties"] is Dictionary<string, object> && conversionOptions.SupportPatternProperties)
            {
                inputSchema = ConvertPatternProperties(inputSchema, PatternPropertiesHandler);
            }

            // If there are unsupported properties, remove them
            if (unsupportedProperties != null)
            {
                foreach (var unsupported in unsupportedProperties)
                {
                    inputSchema.Remove(unsupported);
                }
            }

            // Return the converted schema
            return inputSchema;
        }

        /// <summary>
        /// Converts properties by removing specified properties and converting the remaining ones.
        /// </summary>
        /// <param name="inputProperties">The properties to be converted.</param>
        /// <param name="conversionOptions">The options for the conversion process.</param>
        /// <returns>A dictionary representing the converted properties.</returns>
        public static Dictionary<string, object> ConvertProperties(Dictionary<string, object> inputProperties, OpenApiToJsonSchemaConverterOptions conversionOptions)
        {
            // Initialize a new dictionary to hold the converted properties
            Dictionary<string, object> convertedProperties = new Dictionary<string, object>();

            // Iterate over each property in the input properties
            foreach (var key in inputProperties.Keys)
            {
                // Get the current property
                var property = inputProperties[key];

                // If there are properties specified for removal in the options, check if the current property is one of them
                if (conversionOptions?.RemoveProps != null)
                {
                    var removeProperty = false;
                    foreach (var prop in conversionOptions.RemoveProps)
                    {
                        if (!(property is Dictionary<string, object> propertyDict)
                            || !propertyDict.TryGetValue(prop, out var propValue) || !(bool)propValue)
                        {
                            continue;
                        }

                        // If the current property is specified for removal, set the flag to true
                        removeProperty = true;
                        break;
                    }

                    // If the flag is set, skip the current iteration and move on to the next property
                    if (removeProperty)
                    {
                        continue;
                    }
                }

                // Convert the current property and add it to the converted properties
                convertedProperties[key] = ConvertSchema((Dictionary<string, object>)property, conversionOptions);
            }

            // Return the converted properties
            return convertedProperties;
        }

        /// <summary>
        /// Validates a type by checking if it is one of the valid types.
        /// </summary>
        /// <param name="typeToValidate">The type to be validated.</param>
        /// <exception cref="InvalidTypeError">Thrown when the type is not valid.</exception>
        public static void ValidateType(string typeToValidate)
        {
            // Define a list of valid types
            var validTypes = new List<string> { "integer", "number", "string", "boolean", "object", "array" };

            // If the type to validate is not null and not in the list of valid types, throw an exception
            if (typeToValidate != null && !validTypes.Contains(typeToValidate))
            {
                throw new InvalidTypeError($"Type \"{typeToValidate}\" is not a valid type");
            }
        }

        /// <summary>
        /// Converts types in the schema based on the provided options.
        /// </summary>
        /// <param name="schema">The schema to be converted.</param>
        /// <param name="options">The options for the conversion process.</param>
        /// <returns>A dictionary representing the converted schema.</returns>
        public static Dictionary<string, object> ConvertTypes(Dictionary<string, object> schema, OpenApiToJsonSchemaConverterOptions options)
        {
            // Determine if dates should be converted to DateTime
            var convertDateToDateTime = options?.DateToDateTime == true;

            // If the schema does not contain a type and is not nullable, return the schema as is
            if (!schema.ContainsKey("type") && (!schema.ContainsKey("nullable") || !(bool)schema["nullable"]))
            {
                return schema;
            }

            // If the schema is nullable, add a null type to the list of types
            foreach (var structKey in new List<string> { "oneOf", "anyOf" })
            {
                if (schema.ContainsKey(structKey) && schema[structKey] is List<Dictionary<string, object>> structList)
                {
                    structList.Add(new Dictionary<string, object> { { "type", "null" } });
                }
            }

            // If the schema is of type string and has a date format, convert the format to date-time
            if (schema["type"].ToString() == "string" && schema.ContainsKey("format") && schema["format"].ToString() == "date" && convertDateToDateTime)
            {
                schema["format"] = "date-time";
            }

            // If the schema does not contain a format, remove the format key
            if (!schema.ContainsKey("format"))
            {
                schema.Remove("format");
            }

            // If the schema is nullable, add null to the list of types
            if (schema.ContainsKey("nullable") && (bool)schema["nullable"])
            {
                schema["type"] = new List<string> { schema["type"].ToString(), "null" };
            }

            // If the schema is of type enum, add integer to the list of types
            if (schema["type"].ToString() == "enum")
            {
                schema["type"] = new List<string> { schema["type"].ToString(), "integer" };
            }

            // Return the converted schema
            return schema;
        }

        /// <summary>
        /// Converts pattern properties in the schema using the provided handler.
        /// </summary>
        /// <param name="schema">The schema to be converted.</param>
        /// <param name="handler">The function to handle the conversion.</param>
        /// <returns>A dictionary representing the converted schema.</returns>
        public static Dictionary<string, object> ConvertPatternProperties(Dictionary<string, object> schema, Func<Dictionary<string, object>, Dictionary<string, object>> handler)
        {
            // Replace the x-patternProperties key with patternProperties
            schema["patternProperties"] = schema["x-patternProperties"];
            schema.Remove("x-patternProperties");

            // Return the schema after applying the handler
            return handler(schema);
        }

        /// <summary>
        /// Handles pattern properties in the schema.
        /// </summary>
        /// <param name="schema">The schema to be handled.</param>
        /// <returns>A dictionary representing the handled schema.</returns>
        public static Dictionary<string, object> PatternPropertiesHandler(Dictionary<string, object> schema)
        {
            // Get the pattern properties and additional properties from the schema
            var patternProperties = (Dictionary<string, object>)schema["patternProperties"];
            var additionalProperties = schema.TryGetValue("additionalProperties", out var value) ? value : null;

            // If additional properties match any pattern property, set additional properties to false
            if (additionalProperties is Dictionary<string, object> && patternProperties.Keys.Any(pattern => patternProperties[pattern] == additionalProperties))
            {
                schema["additionalProperties"] = false;
            }

            schema["additionalProperties"] = true;

            // Return the handled schema
            return schema;
        }

        /// <summary>
        /// Resolves unsupported properties by excluding the ones to retain.
        /// </summary>
        /// <param name="notSupported">The list of unsupported properties.</param>
        /// <param name="toRetain">The list of properties to retain.</param>
        /// <returns>A list of resolved unsupported properties.</returns>
        public static List<string> ResolveNotSupported(List<string> notSupported, List<string> toRetain) => notSupported.Except(toRetain).ToList();

        /// <summary>
        /// Cleans required properties by excluding the ones not present in the properties.
        /// </summary>
        /// <param name="required">The list of required properties.</param>
        /// <param name="properties">The dictionary of properties.</param>
        /// <returns>A list of cleaned required properties.</returns>
        public static List<string> CleanRequired(List<string> required, Dictionary<string, object> properties) => required.Where(properties.ContainsKey).ToList();
    }
}
