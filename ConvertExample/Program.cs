using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Models;

using OpenApiToJsonSchemaConverter;

string openApiSchema = @"
{
  'openapi': '3.0.0',
  'info': {
    'version': '1.0.0',
    'title': 'Sample API',
    'description': 'A sample API to illustrate OpenAPI schemas'
  },
  'paths': {
    '/users': {
      'get': {
        'summary': 'Get all users',
        'responses': {
          '200': {
            'description': 'A list of users',
            'content': {
              'application/json': {
                'schema': {
                  'type': 'array',
                  'items': {
                    'type': 'object',
                    'properties': {
                      'id': {
                        'type': 'integer'
                      },
                      'name': {
                        'type': 'string'
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}";


var openApiReader = new OpenApiStringReader();
OpenApiDocument openApiDocument = openApiReader.Read(openApiSchema, out var diagnostic);
Console.WriteLine(openApiDocument.ConvertOpenApiSchemaToJsonSchema());
