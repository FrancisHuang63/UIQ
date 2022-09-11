using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class Member
    {
        [JsonPropertyName("member_id")]
        public int MemberId { get; set; }

        [JsonPropertyName("model_id")]
        public int ModelId { get; set; }

        [JsonPropertyName("member_name")]
        public string MemberName { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("model_group")]
        public string ModelGroup { get; set; }

        [JsonPropertyName("member_path")]
        public string MemberPath { get; set; }

        [JsonPropertyName("member_position")]
        public int MemberPosition { get; set; }

        [JsonPropertyName("member_dtg_value")]
        public int MemberDtgValue { get; set; }

        [JsonPropertyName("reset_model")]
        public string ResetModel { get; set; }

        [JsonPropertyName("dtg_adjust")]
        public string DtgAdjust { get; set; }

        [JsonPropertyName("fix_failed_model")]
        public string FixFailedModel { get; set; }

        [JsonPropertyName("submit_model")]
        public string SubmitModel { get; set; }

        [JsonPropertyName("fix_failed_target_directory")]
        public string FixFailedTargetDirectory { get; set; }

        [JsonPropertyName("maintainer_status")]
        public int MaintainerStatus { get; set; }

        [JsonPropertyName("normal_pre_time")]
        public int NormalPreTime { get; set; }

        [JsonPropertyName("typhoon_pre_time")]
        public int TyphoonPreTime { get; set; }

        [JsonPropertyName("typhoon_model")]
        public int TyphoonModel { get; set; }
    }
}