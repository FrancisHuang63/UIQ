using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class UserModel
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("group_id")]
        public int GroupId { get; set; }

        [JsonPropertyName("passwd")]
        public string Password { get; set; }
    }
}