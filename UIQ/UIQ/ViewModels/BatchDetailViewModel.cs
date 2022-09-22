using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class BatchDetailViewModel
    {
        [JsonPropertyName("batch_name")]
        public string Batch_Name { get; set; }
        
        [JsonPropertyName("setting_time")]
        public int Setting_Time { get; set; }
        
        [JsonPropertyName("history_time")]
        public string History_Time { get; set; }
    }

    public class BatchListViewModel
    {
        [JsonPropertyName("batch_name")]
        public string Batch_Name { get; set; }

        [JsonPropertyName("batch_time")]
        public int Batch_Time { get; set; }

        [JsonPropertyName("batch_type")]
        public string Batch_Type { get; set; }

        [JsonPropertyName("batch_dtg")]
        public string Batch_Dtg { get; set; }
    }

    public class BatchDetailViewModelSearchParameter
    {
        public string ModelName { get; set; }

        public string MemberName { get; set; }

        public string Nickname { get; set; }

        public string RunType { get; set; }

        public string CronMode { get; set; }

        public int TyphoonMode { get; set; }

        public string Round { get; set; }

        public string BatchName { get; set; }
    }
}
