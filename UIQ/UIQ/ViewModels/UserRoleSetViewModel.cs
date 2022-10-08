using System.Text.Json.Serialization;
using UIQ.Models;

namespace UIQ.ViewModels
{
    public class UserRoleSetViewModel : User
    {
        [JsonPropertyName("is_selected")]
        public bool Is_Selected { get; set; }
    }
}
