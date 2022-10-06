using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
	public class UploadFile
	{
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("file_id")]
        public int? file_id { get; set; }

        [JsonPropertyName("file_name")]
        public string? file_name { get; set; }

        [JsonPropertyName("file_path")]
        public string? file_path { get; set; }

        [JsonPropertyName("create_datetime")]
        public string? create_datetime { get; set; }
    }
}
