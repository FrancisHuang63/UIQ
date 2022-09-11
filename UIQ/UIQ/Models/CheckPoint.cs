using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class CheckPoint
    {
        [JsonPropertyName("check_id")]
        public int CheckId { get; set; }

        [JsonPropertyName("member_id")]
        public int MemberId { get; set; }

        [JsonPropertyName("batch_id")]
        public int BatchId { get; set; }

        [JsonPropertyName("shell_name")]
        public string ShellName { get; set; }

        [JsonPropertyName("tolerance_time")]
        public int ToleranceTime { get; set; }
    }
}
