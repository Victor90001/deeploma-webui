using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace webui.Models
{
    public class User
    {
        [JsonPropertyName("login")]
        [FromForm(Name = "login")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
    public class RegisterUser: User
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}
