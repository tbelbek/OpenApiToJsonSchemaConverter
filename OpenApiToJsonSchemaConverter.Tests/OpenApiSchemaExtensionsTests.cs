using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace OpenApiToJsonSchemaConverter.Tests;

[TestFixture]
public class OpenApiSchemaExtensionsTests
{
    private OpenApiDocument _apiDocument;

    [SetUp]
    public void Setup()
    {
        this._apiDocument = new OpenApiDocument { Components = new OpenApiComponents { Schemas = new Dictionary<string, OpenApiSchema> { { "TestSchema", new OpenApiSchema { Type = "string" } } } } };
    }

    [Test]
    public void ConvertOpenApiSchemaToJsonSchema_ShouldReturnListOfJSchema()
    {
        var result = this._apiDocument.ConvertOpenApiSchemaToJsonSchema();

        Assert.That(result, Is.TypeOf<List<JSchema>>());
        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public void ToJsonSchema_ShouldReturnJSchema()
    {
        var openApiSchema = new OpenApiSchema { Type = "string" };
        var result = openApiSchema.ToJsonSchema();

        Assert.That(result, Is.TypeOf<JSchema>());
    }

    [Test]
    public void ParseComponentSchemas_ShouldReturnDictionary()
    {
        var result = this._apiDocument.ParseComponentSchemas();

        Assert.That(result, Is.TypeOf<Dictionary<string, Dictionary<string, JObject>>>());
        Assert.That(result.Count, Is.EqualTo(1));
    }
}