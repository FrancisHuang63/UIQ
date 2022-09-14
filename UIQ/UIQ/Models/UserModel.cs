using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class UserModel
    {
        [JsonPropertyName("user_id")]
        public int User_Id { get; set; }

        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("group_id")]
        public int Group_Id { get; set; }

        [JsonPropertyName("passwd")]
        public string Passwd { get; set; }
    }
}