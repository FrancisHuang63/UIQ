using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class CheckPointInfoViewModel
    {
        [JsonPropertyName("md_id")]
        public int Md_Id { get; set; }

        [JsonPropertyName("md_name")]
        public string Md_Name { get; set; }

        [JsonPropertyName("mb_name")]
        public string Mb_Name { get; set; }

        [JsonPropertyName("mb_acnt")]
        public string Mb_Acnt { get; set; }

        [JsonPropertyName("batch_name")]
        public string Batch_Name { get; set; }

        [JsonPropertyName("run_type")]
        public string Run_Type { get; set; }

        [JsonPropertyName("round")]
        public string Round { get; set; }

        [JsonPropertyName("cron_mode")]
        public string Cron_Mode { get; set; }

        [JsonPropertyName("sh_name")]
        public string Sh_Name { get; set; }
    }    
}