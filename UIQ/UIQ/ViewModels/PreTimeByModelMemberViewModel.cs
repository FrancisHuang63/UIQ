using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class PreTimeByModelMemberViewModel
    {
        [JsonPropertyName("model_name")]
        public string ModelName { get; set; }

        [JsonPropertyName("member_name")]
        public string MemberName { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("typhoon_pre_time")]
        public int TyphoonPreTime { get; set; }

        [JsonPropertyName("normal_pre_time")]
        public int NormalPreTime { get; set; }

        [JsonPropertyName("cron_group")]
        public string CronGroup { get; set; }

        [JsonPropertyName("group_validation")]
        public int GroupValidation { get; set; }
    }
}
