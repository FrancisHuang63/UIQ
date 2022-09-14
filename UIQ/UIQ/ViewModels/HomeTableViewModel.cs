namespace UIQ.ViewModels
{
    public class HomeTableViewModel : ModelConfigViewModel
    {
        public bool AlertFlag { get; set; }

        public string TdText { get { return $"{Member_Name} ({Nickname})"; } }

        public string TdGroupMember { get { return $"{Member_Name}({Nickname})"; } }

        public string TdClass 
        { 
            get
            {
                if (Status.ToUpper() == "FAIL" && AlertFlag) return "FAIL";

                switch (Comment.ToLower())
                {
                    case "delay 10+ mins": return "delay10";
                    case "delay 30+ mins": return "delay20"; 
                    case "delay 1hr+": return "delay1h";
                    case "halt 5min+": return "delay1h_s";
                    case "halt 30min+":
                    case "halt 1hr+": 
                        return "delay2h_s";
                    case "halt 2hr+": return "delay3h_s";
                    case "cancelled": return "cancelled";
                }

                if (Status.ToUpper() == "RUNNING") return "RUNNING";

                return string.Empty;
            }
        }

        public string Href { get; set; }


        public HomeTableViewModel()
        {

        }

        public HomeTableViewModel(ModelConfigViewModel modelConfig)
        {
            var properties = this.GetType().GetProperties();
            foreach (var property in properties)
            {
                if(modelConfig.GetType().GetProperties().Any(x => x.Name == property.Name))
                {
                    var val = property.GetValue(modelConfig, null);
                    property.SetValue(this, val);
                }
            }
        }
    }
}
