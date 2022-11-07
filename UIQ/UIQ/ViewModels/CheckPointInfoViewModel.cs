using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class CheckPointInfoViewModel
    {
        [JsonPropertyName("model_id")]
        public int Model_Id { get; set; }

        [JsonPropertyName("model_name")]
        public string Model_Name { get; set; }

        [JsonPropertyName("member_name")]
        public string Member_Name { get; set; }

        [JsonPropertyName("member_account")]
        public string Member_Account { get; set; }

        [JsonPropertyName("batch_name")]
        public string Batch_Name { get; set; }

        [JsonPropertyName("run_type")]
        public string Run_Type { get; set; }

        [JsonPropertyName("round")]
        public string Round { get; set; }

        [JsonPropertyName("cron_mode")]
        public string Cron_Mode { get; set; }

        [JsonPropertyName("shell_name")]
        public string Shell_Name { get; set; }
    }    
}