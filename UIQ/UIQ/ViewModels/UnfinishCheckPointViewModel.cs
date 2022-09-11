using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class UnfinishCheckPointViewModel
    {
        [JsonPropertyName("avg_execution_time")]
        public DateTime AvgExecutionTime { get; set; }

        [JsonPropertyName("batch_name")]
        public string BatchName { get; set; }

        [JsonPropertyName("shell_name")]
        public string ShellName { get; set; }
    }
}
