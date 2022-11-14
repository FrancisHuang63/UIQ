using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class FullPathViewModel
    {
        [JsonPropertyName("acnt")]
        public string Acnt { get; set; }

        [JsonPropertyName("member_path")]
        public string Member_Path { get; set; }
    }
}
