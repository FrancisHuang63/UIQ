using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class Work
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("work_id")]
        public int Work_Id { get; set; }

        [JsonPropertyName("work_name")]
        public string Work_Name { get; set; }
    }
}
