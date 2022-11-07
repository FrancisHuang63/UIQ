using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class ExecutionTimeResult
    {
        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("member")]
        public string Member { get; set; }

        [JsonPropertyName("batch_name")]
        public string Batch_Name { get; set; }

        [JsonPropertyName("shell_name")]
        public string Shell_Name { get; set; }

        [JsonPropertyName("run_type")]
        public string Run_Type { get; set; }

        [JsonPropertyName("typhoon_mode")]
        public int Typhoon_Mode { get; set; }

        [JsonPropertyName("cron_mode")]
        public string Cron_Mode { get; set; }

        [JsonPropertyName("round")]
        public string Round { get; set; }

        [JsonPropertyName("avg_execution_time")]
        public int Avg_Execution_Time { get; set; }
    }
}