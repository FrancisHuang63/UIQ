using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class PreTimeByModelMemberViewModel
    {
        [JsonPropertyName("model_name")]
        public string Model_Name { get; set; }

        [JsonPropertyName("member_name")]
        public string Member_Name { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("typhoon_pre_time")]
        public int Typhoon_Pre_Time { get; set; }

        [JsonPropertyName("normal_pre_time")]
        public int Normal_Pre_Time { get; set; }

        [JsonPropertyName("cron_group")]
        public string Cron_Group { get; set; }

        [JsonPropertyName("group_validation")]
        public int Group_Validation { get; set; }
    }
}
