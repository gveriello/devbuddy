using System.Text.Json.Serialization;

namespace devbuddy.common.Models
{
    public class AuthenticatedUser
    {
        [JsonPropertyName("sub")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("surname")]
        public string Surname { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}
