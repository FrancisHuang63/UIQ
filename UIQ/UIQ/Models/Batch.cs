using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class Batch
    {
        [JsonPropertyName("batch_id")]
        public int BatchId { get; set; }

        [JsonPropertyName("batch_position")]
        public int BatchPosition { get; set; }

        [JsonPropertyName("member_id")]
        public int MemberId { get; set; }

        [JsonPropertyName("batch_name")]
        public string BatchName { get; set; }

        [JsonPropertyName("batch_type")]
        public string BatchType { get; set; }

        [JsonPropertyName("batch_dtg")]
        public string BatchDtg { get; set; }

        [JsonPropertyName("batch_time")]
        public int BatchTime { get; set; }

        [JsonPropertyName("batch_relay")]
        public int BatchRelay { get; set; }
    }
}
