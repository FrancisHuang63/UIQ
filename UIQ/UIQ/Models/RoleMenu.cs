using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class RoleMenu
    {
        [JsonPropertyName("role_id")]
        public int Role_Id { get; set; }

        [JsonPropertyName("menu_id")]
        public int Menu_Id { get; set; }

        public RoleMenu(int roleId, int menuId)
        {
            Role_Id = roleId;
            Menu_Id = menuId;
        }

        public RoleMenu()
        {

        }
    }
}