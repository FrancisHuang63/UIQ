using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class ShowCheckPointInfoViewModel : MemberCheckPoint
    {
        [JsonPropertyName("avg_time")]
        public IEnumerable<AvgTime> Avg_Time { get; set; }

        public ShowCheckPointInfoViewModel(MemberCheckPoint checkPoint)
        {
            Md_Name = checkPoint.Md_Name;
            Mb_Name = checkPoint.Mb_Name;
            Nickname = checkPoint.Nickname;
            Acnt = checkPoint.Acnt;
            Batch_Id = checkPoint.Batch_Id;
            Batch_Name = checkPoint.Batch_Name;
            Batch_Type = checkPoint.Batch_Type;
            Batch_Dtg = checkPoint.Batch_Dtg;
            Shell_Name = checkPoint.Shell_Name;
            Tolerance_Time = checkPoint.Tolerance_Time;
        }
    }

    public class MemberCheckPoint
    {
        [JsonPropertyName("md_name")]
        public string Md_Name { get; set; }

        [JsonPropertyName("mb_name")]
        public string Mb_Name { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("acnt")]
        public string Acnt { get; set; }

        [JsonPropertyName("batch_id")]
        public int Batch_Id { get; set; }

        [JsonPropertyName("batch_name")]
        public string Batch_Name { get; set; }

        [JsonPropertyName("batch_type")]
        public string Batch_Type { get; set; }

        [JsonPropertyName("batch_dtg")]
        public string Batch_Dtg { get; set; }

        [JsonPropertyName("shell_name")]
        public string Shell_Name { get; set; }

        [JsonPropertyName("tolerance_time")]
        public int Tolerance_Time { get; set; }
    }

    public class AvgTime
    {
        [JsonPropertyName("typhoon_mode")]
        public int Typhoon_Mode { get; set; }

        [JsonPropertyName("avg_execution_time")]
        public int Avg_Execution_Time { get; set; }
    }
}
