using System.Text.Json.Serialization;
using UIQ.Models;

namespace UIQ.ViewModels
{
    public class MenuRoleSetViewModel : Menu
    {
        [JsonPropertyName("is_selected")]
        public bool Is_Selected { get; set; }
    }
}