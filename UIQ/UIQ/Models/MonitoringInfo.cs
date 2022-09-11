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
        public string CompleteRunType { get; set; }

        [JsonPropertyName("run_type")]
        public string Run_type { get; set; }

        [JsonPropertyName("cron_mode")]
        public CronModeEnum CronMode { get; set; }

        [JsonPropertyName("typhoon_mode")]
        public TyphoonModeEnum TyphoonMode { get; set; }

        [JsonPropertyName("manual")]
        public int Manual { get; set; }

        [JsonPropertyName("start_flag")]
        public int StartFlag { get; set; }

        [JsonPropertyName("stage_flag")]
        public int StageFlag { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("sms_name")]
        public string SmsName { get; set; }

        [JsonPropertyName("sms_time")]
        public DateTime SmsTime { get; set; }

        [JsonPropertyName("start_time")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public DateTime EndTime { get; set; }

        [JsonPropertyName("pre_start")]
        public DateTime PreStart { get; set; }

        [JsonPropertyName("pre_end")]
        public DateTime PreEnd { get; set; }

        [JsonPropertyName("run_end")]
        public DateTime RunEnd { get; set; }

        [JsonPropertyName("shell_name")]
        public string ShellName { get; set; }

        [JsonPropertyName("shell_time")]
        public DateTime ShellTime { get; set; }

        [JsonPropertyName("error_message")]
        public string ErrorMessage { get; set; }
    }
}