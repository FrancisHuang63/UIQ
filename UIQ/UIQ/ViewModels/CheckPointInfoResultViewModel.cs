using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class CheckPointInfoResultViewModel
    {
        [JsonPropertyName("shell_name")]
        public string Shell_Name { get; set; }

        [JsonPropertyName("round")]
        public string Round { get; set; }

        [JsonPropertyName("typhoon_mode")]
        public int Typhoon_Mode { get; set; }

        [JsonPropertyName("avg_execution_time")]
        public int Avg_Execution_Time { get; set; }
    }
}
