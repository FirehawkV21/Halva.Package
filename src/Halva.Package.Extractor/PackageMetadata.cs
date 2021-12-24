using System.Text.Json.Serialization;

namespace Halva.Package.Bootstrapper
{
    public class PackageMetadata
    {
        public Packages PackageList { get; set; } = new();
        public class Packages
        {
            [JsonPropertyName("GameAssets")]
            public int AssetsVersion { get; set; } = 0;
            [JsonPropertyName("GameAudio")]
            public int AudioVersion { get; set; } = 0;
            [JsonPropertyName("GameDB")]
            public int DatabaseVersion { get; set; } = 0;
            [JsonPropertyName("GameEngine")]
            public int EngineVersion { get; set; } = 0;
        }
    }
}
