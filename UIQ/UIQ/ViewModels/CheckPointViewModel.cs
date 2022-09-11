using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using UIQ.Enums;

namespace UIQ.ViewModels
{
    public class CheckPointViewModel
    {
        [JsonPropertyName("model_name")]
        public string ModelName { get; set; }

        [JsonPropertyName("member_name")]
        public string MemberName { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("batch_name")]
        public string BatchName { get; set; }

        [JsonPropertyName("batch_type")]
        public string BatchType { get; set; }

        [JsonPropertyName("batch_dtg")]
        public string BatchDtg { get; set; }

        [JsonPropertyName("check_id")]
        public int CheckId { get; set; }

        [JsonPropertyName("shell_name")]
        public string ShellName { get; set; }

        [JsonPropertyName("tolerance_time")]
        public int ToleranceTime { get; set; }


        [NotMapped]
        public int AvgExecutionTime { get; set; }

        [NotMapped]
        public int PredictEndSec { get { return AvgExecutionTime + (ToleranceTime * 60); } }
    }

    public class CheckPointViewModelSearch
    {
        public string ModelName { get; set; }

        public string MemberName { get; set; }
        
        public string Account { get; set; }

        public string CompleteRunType { get; set; }

        public string CronMode { get; set; }

        public int TyphoonMode { get; set; }
        
        public string Dtg { get; set; }

        public string DtgHour { get; set; }
    }
}
