using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class CheckPoint
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("check_id")]
        public int Check_Id { get; set; }

        [JsonPropertyName("member_id")]
        public int Member_Id { get; set; }

        [JsonPropertyName("batch_id")]
        public int Batch_Id { get; set; }

        [JsonPropertyName("shell_name")]
        public string Shell_Name { get; set; }

        [JsonPropertyName("tolerance_time")]
        public int Tolerance_Time { get; set; }
    }
}