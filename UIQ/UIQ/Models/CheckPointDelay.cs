using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using UIQ.Enums;

namespace UIQ.Models
{
    public class CheckPointDelay
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("check_id")]
        public int Check_Id { get; set; }

        [JsonPropertyName("is_processed")]
        public bool Is_Processed { get; set; }

        [JsonPropertyName("run_type")]
        public string Run_Type { get; set; }

        [JsonPropertyName("cron_mode")]
        public string Cron_Mode { get; set; }

        [JsonPropertyName("typhoon_mode")]
        public TyphoonModeEnum Typhoon_Mode { get; set; }

        [JsonPropertyName("dtg")]
        public string Dtg { get; set; }

        [JsonPropertyName("model_start_time")]
        public DateTime Model_Start_Time { get; set; }

        [JsonPropertyName("predicted_end_time")]
        public DateTime Predicted_End_Time { get; set; }

        [JsonPropertyName("monitoring_time")]
        public DateTime Monitoring_Time { get; set; }
    }
}