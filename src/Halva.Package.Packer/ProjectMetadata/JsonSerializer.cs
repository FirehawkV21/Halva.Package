using System.Text.Json.Serialization;

namespace Halva.Package.Packer
{
    [JsonSerializable(typeof(ProjectMetadata))]
    public partial class JsonSerializer : JsonSerializerContext
    {
    }
}
