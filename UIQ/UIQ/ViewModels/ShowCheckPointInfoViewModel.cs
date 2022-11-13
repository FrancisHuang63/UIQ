using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class ShowCheckPointInfoViewModel : MemberCheckPoint
    {
        [JsonPropertyName("avg_time")]
        public IEnumerable<AvgTime> Avg_Time { get; set; }

        public ShowCheckPointInfoViewModel(MemberCheckPoint checkPoint)
        {
            Model_Name = checkPoint.Model_Name;
            Member_Name = checkPoint.Member_Name;
            Nickname = checkPoint.Nickname;
            Account = checkPoint.Account;
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
        [JsonPropertyName("model_name")]
        public string Model_Name { get; set; }

        [JsonPropertyName("member_name")]
        public string Member_Name { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("account")]
        public string Account { get; set; }

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
