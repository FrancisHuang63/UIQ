using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class CronTab
    {
        [JsonPropertyName("member_id")]
        public int Member_Id { get; set; }

        [JsonPropertyName("start_time")]
        public int Start_Time { get; set; }

        [JsonPropertyName("cron_group")]
        public string Cron_Group { get; set; }

        [JsonPropertyName("group_validation")]
        public string Group_Validation { get; set; }

        [JsonPropertyName("master_group")]
        public int Master_Group { get; set; }
    }
}
