using System.Text.Json.Serialization;

namespace Finite_State_Machine_Designer.Configuration.ExternalLogins.Github
{
    public class GitEmail
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("primary")]
        public bool Primary { get; set; }

        [JsonPropertyName("verified")]
        public bool Verified { get; set; }

        [JsonPropertyName("visibility")]
        public string? Visibility { get; set; }
    }
}
