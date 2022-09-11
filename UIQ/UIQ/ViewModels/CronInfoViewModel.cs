using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class CronInfoViewModel
    {
        [JsonPropertyName("model_member_nick")]
        public string ModelMemberNick { get; set; }

        [JsonPropertyName("start")]
        public string Start { get; set; }
    }
}
