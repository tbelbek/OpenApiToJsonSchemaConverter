# OpenApiToJsonSchemaConverter

This project is a library for converting OpenAPI schemas to JSON schemas. It provides a set of methods to handle the
conversion process, including handling of pattern properties, making properties nullable, and preparing schemas for
conversion.

## Installation

To use this library, you can add it as a reference to your project.

## Usage

Here are some examples of how to use the methods provided by this library:

### ConvertProperties

This method is used to convert properties from an OpenAPI schema to a JSON schema.

```

var inputProperties = new Dictionary<string, object> { { "property1", new Dictionary<string, object> { { "type", "string" } } } };
var options = new ConversionOptions();
var result = OpenApiSchemaToJsonSchema.ConvertProperties(inputProperties, options);

```

### ConvertToJSONSchema

This method is used to convert an entire OpenAPI schema to a JSON schema.

```
var openApiSchema = new OpenApiSchema { Type = "string" };
var options = new ConversionOptions();
var result = OpenApiSchemaToJsonSchema.ConvertToJSONSchema(openApiSchema, options);
```

### ConvertSchema

This method is used to convert a schema from OpenAPI to JSON schema.

```
var inputSchema = new Dictionary<string,  object> { { "type", "string" } };
var options = new ConversionOptions();
var result = OpenApiSchemaToJsonSchema.ConvertSchema(inputSchema, options);
```

### ToJsonSchema

This method is used to convert an OpenAPI schema to a JSON schema.

```
var openApiSchema = new OpenApiSchema { Type = "string" };
var result = openApiSchema.ToJsonSchema();
```

### ParseComponentSchemas

This method is used to parse the component schemas from an OpenAPI document.

```
var apiDocument = new OpenApiDocument();
var result = apiDocument.ParseComponentSchemas();
```

### ConvertOpenApiSchemaToJsonSchema

This method is used to convert an OpenAPI schema to a JSON schema.

```
var apiDocument = new OpenApiDocument();
var result = apiDocument.ConvertOpenApiSchemaToJsonSchema();
```

### MakeAllPropertiesNullable

This method is used to make all properties of a schema nullable.

```
var schema = new JObject();
var result = schema.MakeAllPropertiesNullable();
```

### PrepareForConversion

This method is used to prepare a schema for conversion from OpenAPI to JSON schema.

```
var inputSchema = new Dictionary<string,  object> { { "type", "string" } };
var options = new ConversionOptions();
var (preparedSchema, conversionOptions) = OpenApiSchemaToJsonSchema.PrepareForConversion(inputSchema, options);
```

### ConvertPatternProperties

This method is used to convert pattern properties from an OpenAPI schema to a JSON schema.

```
var schema = new Dictionary<string,  object> { { "x-patternProperties", new Dictionary<string,  object>() } };
var handler = new PatternPropertyHandler(); var result = OpenApiSchemaToJsonSchema.ConvertPatternProperties(schema, handler);
```

Please replace `options`, `handler`, `openApiSchema`, and `apiDocument` with your own instances.

## Testing

This project includes a set of unit tests that you can run to verify the functionality of the library.

## Contributing

Contributions are welcome. Please submit a pull request with any enhancements.

## License

This project is licensed under the MIT License.

