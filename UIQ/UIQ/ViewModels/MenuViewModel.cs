using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using UIQ.Models;

namespace UIQ.ViewModels
{
    public class MenuViewModel : Menu
    {
        [NotMapped]
        public List<MenuViewModel> ChildItems { get; set; }
    }
}