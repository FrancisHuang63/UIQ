using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class ModelTimeViewModel
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("member")]
        public string Member { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("avg_execution_time")]
        public long Avg_Execution_Time { get; set; }

        [JsonPropertyName("run_type")]
        public string Run_Type { get; set; }

        [JsonPropertyName("cron_mode")]
        public string Cron_Mode { get; set; }

        [JsonPropertyName("typhoon_mode")]
        public int Typhoon_Mode { get; set; }

        [JsonPropertyName("round")]
        public string Round { get; set; }

        [JsonPropertyName("batch_time")]
        public int Batch_Time { get; set; }
    }
}
