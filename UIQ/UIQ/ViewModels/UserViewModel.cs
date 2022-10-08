using System.Text.Json.Serialization;
using UIQ.Models;

namespace UIQ.ViewModels
{
    public class UserViewModel : User
    {
        [JsonPropertyName("group_name")]
        public string Group_Name { get; set; }

        [JsonPropertyName("roles")]
        public string Roles { get; set; }
    }
}