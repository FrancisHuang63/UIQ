using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class ArchiveViewModel
    {
        [JsonPropertyName("model_id")]
        public int Model_Id { get; set; }

        [JsonPropertyName("model_name")]
        public string Model_Name { get; set; }

        [JsonPropertyName("member_name")]
        public string Member_Name { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("account")]
        public string Account { get; set; }
    }
}
