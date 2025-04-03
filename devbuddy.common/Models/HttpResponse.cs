using System.Text.Json.Serialization;

namespace devbuddy.common.Models
{
    public class HttpResponse<TData>
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        [JsonPropertyName("data")]
        public TData? Data { get; set; }

        public bool Success => this.Status == "success";
    }
}
