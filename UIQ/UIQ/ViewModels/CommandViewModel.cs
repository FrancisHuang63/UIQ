using System.Text.Json.Serialization;
using UIQ.Models;

namespace UIQ.ViewModels
{
    public class CommandViewModel
    {
        [JsonPropertyName("c_id")]
        public int? C_Id { get; set; }

        [JsonPropertyName("c_name")]
        public string C_Name { get; set; }

        [JsonPropertyName("c_desc")]
        public string C_Desc { get; set; }
        
        [JsonPropertyName("c_content")]
        public string C_Content { get; set; }

        [JsonPropertyName("c_pwd")]
        public string C_Pwd { get; set; }

        [JsonPropertyName("execution_time")]
        public int Execution_Time { get; set; }

        [JsonPropertyName("c_example")]
        public string C_Example { get; set; }

        public CommandViewModel()
        {

        }

        public CommandViewModel(Command command)
        {
            C_Id = command.Command_Id;
            C_Name = command.Command_Name;
            C_Desc = command.Command_Desc;
            C_Content = command.Command_Content;
            C_Pwd = command.Command_Pwd;
            Execution_Time = command.Execution_Time;
            C_Example = command.Command_Example;
        }
    }
}