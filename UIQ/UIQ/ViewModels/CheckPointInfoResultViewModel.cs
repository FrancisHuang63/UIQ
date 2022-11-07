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

    public class CheckPointSaveViewModel
    {
        [JsonPropertyName("model_id")]
        public int Model_Id { get; set; }

        [JsonPropertyName("member_id")]
        public int Member_Id { get; set; }

        [JsonPropertyName("batch_id")]
        public int Batch_Id { get; set; }

        [JsonPropertyName("batch_name")]
        public string Batch_Name { get; set; }

        [JsonPropertyName("shell_name")]
        public string Shell_Name { get; set; }

        [JsonPropertyName("tolerance_time")]
        public int Tolerance_Time { get; set; }
    }
}
