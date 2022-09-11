using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class ExecutingShellViewModel
    {
        [JsonPropertyName("batch_name")]
        public string BatchName { get; set; }

        [JsonPropertyName("shell_name")]
        public string ShellName { get; set; }
    }
}
