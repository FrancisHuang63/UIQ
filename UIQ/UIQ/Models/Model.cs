using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class Model
    {
        [JsonPropertyName("model_id")]
        public int ModelId { get; set; }

        [JsonPropertyName("model_name")]
        public string ModelName { get; set; }

        [JsonPropertyName("model_position")]
        public int ModelPosition { get; set; }
    }
}