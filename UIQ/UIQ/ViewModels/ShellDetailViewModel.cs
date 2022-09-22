using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class ShellDetailViewModel
    {
        [JsonPropertyName("batch_name")]
        public string Batch_Name { get; set; }

        [JsonPropertyName("shell_name")]
        public string Shell_Name { get; set; }

        [JsonPropertyName("avg_time_min")]
        public int Avg_Time_Min { get; set; }

        [JsonPropertyName("avg_time_sec")]
        public int Avg_Time_Sec { get; set; }
    }

    public class ShellDetailViewModelSearchParameter : BatchDetailViewModelSearchParameter
    {
       
    }
}
