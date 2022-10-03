using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class Command
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("command_id")]
        public int? Command_Id { get; set; }

        [JsonPropertyName("command_name")]
        public string Command_Name { get; set; }

        [JsonPropertyName("command_desc")]
        public string Command_Desc { get; set; }

        [JsonPropertyName("command_content")]
        public string Command_Content { get; set; }

        [JsonPropertyName("command_pwd")]
        public string Command_Pwd { get; set; }

        [JsonPropertyName("execution_time")]
        public int Execution_Time { get; set; }

        [JsonPropertyName("command_example")]
        public string Command_Example { get; set; }
    }
}
