using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class ArchiveViewModel
    {
        [JsonPropertyName("md_id")]
        public int Md_Id { get; set; }

        [JsonPropertyName("md_name")]
        public string Md_Name { get; set; }

        [JsonPropertyName("mb_name")]
        public string Mb_Name { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("acnt")]
        public string Acnt { get; set; }
    }
}
