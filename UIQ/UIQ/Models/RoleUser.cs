using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class RoleUser
    {
        [JsonPropertyName("role_id")]
        public int Role_Id { get; set; }

        [JsonPropertyName("user_id")]
        public int User_Id { get; set; }

        public RoleUser(int roleId, int userId)
        {
            Role_Id = roleId;
            User_Id = userId;
        }

        public RoleUser()
        {
        }
    }
}