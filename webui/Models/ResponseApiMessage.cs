using System.Text.Json.Serialization;

namespace webui.Models
{
    public class ResponseApiMessage
    {
        [JsonPropertyName("result")]
        public bool Result { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"result = {Result}\nmessage = {Message}";
        }
    }
}
