using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
	public class Role
	{
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("role_id")]
        public int? role_id { get; set; }

        [JsonPropertyName("role_name")]
        public string? role_name { get; set; }

        [JsonPropertyName("create_datetime")]
        public string? create_datetime { get; set; }

        [JsonPropertyName("last_update_datetime")]
        public string? last_update_datetime { get; set; }
        
    }
}
