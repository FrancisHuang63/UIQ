using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class GetShellDelayDataViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("predicted_end_time")]
        public DateTime Predicted_End_Time { get; set; }

        [JsonPropertyName("model_start_time")]
        public DateTime Model_Start_Time { get; set; }

        [JsonPropertyName("run_type")]
        public string Run_Type { get; set; }

        [JsonPropertyName("dtg")]
        public string Dtg { get; set; }

        [JsonPropertyName("batch_name")]
        public string Batch_Name { get; set; }

        [JsonPropertyName("member_name")]
        public string Member_Name { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("shell_name")]
        public string Shell_Name { get; set; }

        [JsonPropertyName("model_name")]
        public string Model_Name { get; set; }
    }
}
