using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FileProcessing.Models
{
    public class TypeInfo
    {
        [JsonPropertyName("TypeName")]
        public string TypeName { get; set; }

        [JsonPropertyName("Propertys")]
        public Dictionary<string, string> Properties { get; set; }
    }

    public class TypeInfosRoot
    {
        [JsonPropertyName("TypeInfos")]
        public List<TypeInfo> TypeInfos { get; set; }
    }
}
