using System.Text.Json.Serialization;

namespace SociallyDistant.WorldObjects
{
    public class ContentPackMetadata
    {
        [JsonPropertyName("name")] public string Name { get; set; } = "Unnamed World";

        [JsonPropertyName("author")] public string Author { get; set; } = string.Empty;

        [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;

        [JsonPropertyName("icon")] public ImageAssetReference Icon { get; set; } = new();

        [JsonPropertyName("wallpaper")] public ImageAssetReference Wallpaper { get; set; } = new();

        [JsonPropertyName("bootLogo")] public ImageAssetReference BootLogo { get; set; } = new();

        [JsonPropertyName("enableEULA")] public bool EnableEula { get; set; } = false;
    }
}