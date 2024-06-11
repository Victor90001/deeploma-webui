using System.Text.Json.Nodes;

namespace webui.Models
{
    public class ResponseApiPoints
    {
        public JsonArray data { get; set; }
        public string? error { get; set; }
    }
}
