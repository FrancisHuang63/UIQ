using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class Data
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("data_id")]
        public int Data_Id { get; set; }

        [JsonPropertyName("data_name")]
        public string Data_Name { get; set; }
    }
}
