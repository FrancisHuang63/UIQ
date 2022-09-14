using System.Text.Json.Serialization;
using UIQ.Enums;

namespace UIQ.Models
{
    public class MonitoringInfo
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("member")]
        public string Member { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("lid")]
        public LidEnum Lid { get; set; }

        [JsonPropertyName("dtg")]
        public string Dtg { get; set; }

        [JsonPropertyName("run")]
        public string Run { get; set; }

        [JsonPropertyName("complete_run_type")]
        public string Complete_Run_Type { get; set; }

        [JsonPropertyName("run_type")]
        public string Run_type { get; set; }

        [JsonPropertyName("cron_mode")]
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
        public DateTime PreStart { get; set; }

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
    }
}