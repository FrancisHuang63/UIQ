using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class Menu
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("menu_id")]
        public int Menu_Id { get; set; }

        [JsonPropertyName("parent_id")]
        public int? Parent_Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("sort")]
        public int Sort { get; set; }
    }
}