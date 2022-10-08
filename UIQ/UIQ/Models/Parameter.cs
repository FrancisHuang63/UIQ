using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class Parameter
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("parameter_id")]
        public string? Parameter_Id { get; set; }

        [JsonPropertyName("parameter_value")]
        public string? Parameter_Value { get; set; }

    }
}
