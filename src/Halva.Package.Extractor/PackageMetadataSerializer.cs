using System.Text.Json.Serialization;

namespace Halva.Package.Bootstrapper;
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(PackageMetadata))]
public partial class PackageMetadataSerializer : JsonSerializerContext
{
}
