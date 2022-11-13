using UIQ.Models;

namespace UIQ.ViewModels
{
    public class CommandViewModel
    {
        public int? C_Id { get; set; }

        public string C_Name { get; set; }

        public string C_Desc { get; set; }

        public string C_Content { get; set; }

        public string C_Pwd { get; set; }

        public int Execution_Time { get; set; }

        public string C_Example { get; set; }

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