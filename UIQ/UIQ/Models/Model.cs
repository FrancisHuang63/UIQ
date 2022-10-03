using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class Model
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("model_id")]
        public int Model_Id { get; set; }

        [JsonPropertyName("model_name")]
        public string Model_Name { get; set; }

        [JsonPropertyName("model_position")]
        public int Model_Position { get; set; }
    }
}