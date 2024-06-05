using Microsoft.OpenApi.Models;

namespace OpenApiToJsonSchemaConverter.Tests;

[TestFixture]
public class OpenApiSchemaToJsonSchemaTests
{
    private OpenApiSchema _openApiSchema;

    private OpenApiToJsonSchemaConverterOptions _options;

    [SetUp]
    public void Setup()
    {
        this._openApiSchema = new OpenApiSchema { Type = "string" };

        this._options = new OpenApiToJsonSchemaConverterOptions();
    }

    [Test]
    public void ConvertToJSONSchema_ShouldReturnDictionary()
    {
        var result = OpenApiSchemaToJsonSchema.ConvertToJSONSchema(this._openApiSchema, this._options);

        Assert.That(result, Is.TypeOf<Dictionary<string, object>>());
        Assert.That(result.ContainsKey("type"), Is.True);
        Assert.That(result["type"], Is.EqualTo("string"));
    }

    [Test]
    public void ConvertSchema_ShouldReturnDictionary()
    {
        var inputSchema = new Dictionary<string, object> { { "type", "string" } };

        var result = OpenApiSchemaToJsonSchema.ConvertSchema(inputSchema, this._options);

        Assert.That(result, Is.TypeOf<Dictionary<string, object>>());
        Assert.That(result.ContainsKey("type"), Is.True);
        Assert.That(result["type"], Is.EqualTo("string"));
    }

    [Test]
    public void ConvertProperties_ShouldReturnDictionary()
    {
        var inputProperties = new Dictionary<string, object>
                                  {
                                      { "property1", new Dictionary<string, object> { { "type", "string" } } }
                                  };

        var result = OpenApiSchemaToJsonSchema.ConvertProperties(inputProperties, this._options);

        Assert.That(result, Is.TypeOf<Dictionary<string, object>>());
        Assert.That(result.ContainsKey("property1"), Is.True);
        Assert.That(result["property1"], Is.TypeOf<Dictionary<string, object>>());
    }
}