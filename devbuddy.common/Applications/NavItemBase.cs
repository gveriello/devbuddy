using System.Text.Json.Serialization;
using devbuddy.common.Enums;

namespace devbuddy.common.Applications
{
    public abstract class NavItemBase
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("apikey")]
        public string ApiKey { get; set; }
    }
}
