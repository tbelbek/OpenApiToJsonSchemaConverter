using System;

namespace OpenApiToJsonSchemaConverter
{
    public class InvalidTypeError : Exception
    {
        public InvalidTypeError(string message) : base(message)
        {
        }
    }
}
