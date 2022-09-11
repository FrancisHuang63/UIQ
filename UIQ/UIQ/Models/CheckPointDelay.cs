using System.Text.Json.Serialization;
using UIQ.Enums;

namespace UIQ.Models
{
    public class CheckPointDelay
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("check_id")]
        public int CheckId { get; set; }

        [JsonPropertyName("is_processed")]
        public bool IsProcessed { get; set; }

        [JsonPropertyName("run_type")]
        public string RunType { get; set; }

        [JsonPropertyName("cron_mode")]
        public string CronMode { get; set; }

        [JsonPropertyName("typhoon_mode")]
        public TyphoonModeEnum TyphoonMode { get; set; }

        [JsonPropertyName("dtg")]
        public string Dtg { get; set; }

        [JsonPropertyName("model_start_time")]
        public DateTime ModelStartTime { get; set; }

        [JsonPropertyName("predicted_end_time")]
        public DateTime PredictedEndTime { get; set; }

        [JsonPropertyName("monitoring_time")]
        public DateTime MonitoringTime { get; set; }
    }
}
