using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class Output
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("output_id")]
        public int Output_Id { get; set; }

        [JsonPropertyName("member_id")]
        public int Member_Id { get; set; }

        [JsonPropertyName("work_id")]
        public int Work_Id { get; set; }

        [JsonPropertyName("model_output")]
        public string Model_Output { get; set; }
    }
}
