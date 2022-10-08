using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
	public class Role
	{
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("role_id")]
        public int Role_Id { get; set; }

        [JsonPropertyName("role_name")]
        public string Role_Name { get; set; }

        [JsonPropertyName("create_datetime")]
        public DateTime Create_DateTime { get; set; }

        [JsonPropertyName("last_update_datetime")]
        public DateTime? Last_Update_DateTime { get; set; }

        public Role(string roleName)
        {
            Role_Name = roleName;
            Create_DateTime = DateTime.Now;
        }

        public Role()
        {

        }
    }
}
