using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class CronSettingViewModel
    {
        [JsonPropertyName("cron_group")]
        public string Cron_Group { get; set; }

        [JsonPropertyName("is_master_group")]
        public bool Is_Master_Group { get; set; }
    }
}
