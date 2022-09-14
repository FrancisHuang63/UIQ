using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class CronInfoViewModel
    {
        [JsonPropertyName("model_member_nick")]
        public string Model_Member_Nick { get; set; }

        [JsonPropertyName("start")]
        public string Start { get; set; }
    }
}
