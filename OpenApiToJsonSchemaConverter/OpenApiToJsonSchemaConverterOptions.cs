using System.Collections.Generic;

namespace OpenApiToJsonSchemaConverter
{
    public class OpenApiToJsonSchemaConverterOptions
    {
        public bool DateToDateTime { get; set; } = false;

        public bool CloneSchema { get; set; } = true;

        public bool SupportPatternProperties { get; set; } = true;

        public List<string> KeepNotSupported { get; set; } = new List<string>();

        public bool RemoveReadOnly { get; set; } = false;

        public bool RemoveWriteOnly { get; set; } = false;

        public List<string> Structs { get; set; } = new List<string>()
                                                        {
                                                            "allOf",
                                                            "anyOf",
                                                            "oneOf",
                                                            "not",
                                                            "items",
                                                            "additionalProperties"
                                                        };

        public List<string> NotSupported { get; set; } = new List<string>();

        public List<string> RemoveProps { get; set; } = new List<string>();
    }
}
