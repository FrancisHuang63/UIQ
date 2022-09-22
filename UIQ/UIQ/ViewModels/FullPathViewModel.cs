using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class FullPathViewModel
    {
        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("member_path")]
        public string Member_Path { get; set; }
    }
}
