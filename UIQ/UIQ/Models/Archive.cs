using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class Archive
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("archive_id")]
        public int Archive_Id { get; set; }

        [JsonPropertyName("member_id")]
        public int Member_Id { get; set; }

        [JsonPropertyName("data_id")]
        public int Data_Id { get; set; }

        [JsonPropertyName("archive_redo")]
        public string Archive_Redo { get; set; }

        [JsonPropertyName("target_directory")]
        public string Target_Directory { get; set; }
    }
}
