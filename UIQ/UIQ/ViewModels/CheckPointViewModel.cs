using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class CheckPointViewModel
    {
        [JsonPropertyName("model_name")]
        public string Model_Name { get; set; }

        [JsonPropertyName("member_name")]
        public string Member_Name { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("batch_name")]
        public string Batch_Name { get; set; }

        [JsonPropertyName("batch_type")]
        public string Batch_Type { get; set; }

        [JsonPropertyName("batch_dtg")]
        public string Batch_Dtg { get; set; }

        [JsonPropertyName("check_id")]
        public int Check_Id { get; set; }

        [JsonPropertyName("shell_name")]
        public string Shell_Name { get; set; }

        [JsonPropertyName("tolerance_time")]
        public int Tolerance_Time { get; set; }

        [NotMapped]
        public int AvgExecutionTime { get; set; }

        [NotMapped]
        public int PredictEndSec
        { get { return AvgExecutionTime + (Tolerance_Time * 60); } }
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