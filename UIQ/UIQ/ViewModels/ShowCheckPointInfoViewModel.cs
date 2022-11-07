using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class ShowCheckPointInfoViewModel : MemberCheckPoint
    {
        [JsonPropertyName("avg_time")]
        public IEnumerable<AvgTime> Avg_Time { get; set; }

        public ShowCheckPointInfoViewModel(MemberCheckPoint memberCheckPoint)
        {
            Model_Name = memberCheckPoint.Model_Name;
            Member_Name = memberCheckPoint.Member_Name;
            Nickname = memberCheckPoint.Nickname;
            Account = memberCheckPoint.Account;
            Batch_Id = memberCheckPoint.Batch_Id;
            Batch_Name = memberCheckPoint.Batch_Name;
            Batch_Type = memberCheckPoint.Batch_Type;
            Batch_Dtg = memberCheckPoint.Batch_Dtg;
            Shell_Name = memberCheckPoint.Shell_Name;
            Tolerance_Time = memberCheckPoint.Tolerance_Time;
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
