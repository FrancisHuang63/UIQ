using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using UIQ.Enums;
using UIQ.Models;

namespace UIQ.ViewModels
{
    public class ModelConfigViewModel : Member
    {
        [JsonPropertyName("model_name")]
        public string Model_Name { get; set; }

        [JsonPropertyName("model_position")]
        public int Model_Position { get; set; }

        [JsonPropertyName("lid")]
        public LidEnum Lid { get; set; }

        [JsonPropertyName("dtg")]
        public string Dtg { get; set; }

        [JsonPropertyName("complete_run_type")]
        public string Complete_Run_Type { get; set; }

        [JsonPropertyName("run_type")]
        public string Run_Type { get; set; }

        [JsonPropertyName("cron_mode")]
        [JsonConverter(typeof(CronModeEnum))]
        public CronModeEnum Cron_Mode { get; set; }

        [JsonPropertyName("typhoon_mode")]
        public TyphoonModeEnum Typhoon_Mode { get; set; }

        [JsonPropertyName("manual")]
        public int Manual { get; set; }

        [JsonPropertyName("start_flag")]
        public int Start_Flag { get; set; }

        [JsonPropertyName("stage_flag")]
        public int Stage_Flag { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("sms_name")]
        public string Sms_Name { get; set; }

        [JsonPropertyName("sms_time")]
        public DateTime Sms_Time { get; set; }

        [JsonPropertyName("start_time")]
        public DateTime Start_Time { get; set; }

        [JsonPropertyName("end_time")]
        public DateTime End_Time { get; set; }

        [JsonPropertyName("pre_start")]
        public DateTime Pre_Start { get; set; }

        [JsonPropertyName("pre_end")]
        public DateTime Pre_End { get; set; }

        [JsonPropertyName("run_end")]
        public DateTime Run_End { get; set; }

        [JsonPropertyName("shell_name")]
        public string Shell_Name { get; set; }

        [JsonPropertyName("shell_time")]
        public DateTime Shell_Time { get; set; }

        [JsonPropertyName("error_message")]
        public string Error_Message { get; set; }


        [NotMapped]
        public string NextRun { get; set; } = string.Empty;

        [NotMapped]
        public int PreTime { get; set; } = 300;

        [NotMapped]
        public string Comment { get; set; }
    }
}