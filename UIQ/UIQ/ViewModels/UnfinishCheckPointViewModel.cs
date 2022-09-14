using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class UnfinishCheckPointViewModel
    {
        [JsonPropertyName("avg_execution_time")]
        public DateTime Avg_Execution_Time { get; set; }

        [JsonPropertyName("batch_name")]
        public string Batch_Name { get; set; }

        [JsonPropertyName("shell_name")]
        public string Shell_Name { get; set; }
    }
}
