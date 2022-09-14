using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class ExecutingShellViewModel
    {
        [JsonPropertyName("batch_name")]
        public string Batch_Name { get; set; }

        [JsonPropertyName("shell_name")]
        public string Shell_Name { get; set; }
    }
}
