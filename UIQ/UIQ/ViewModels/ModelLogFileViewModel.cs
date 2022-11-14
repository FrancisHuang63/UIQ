using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class ModelLogFileViewModel
    {
        [JsonPropertyName("md_name")]
        public string Md_Name { get; set; }

        [JsonPropertyName("md_id")]
        public int Md_Id { get; set; }

        [JsonPropertyName("mb_name")]
        public string Mb_Name { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("acnt")]
        public string Acnt { get; set; }

        [JsonPropertyName("member_dtg_value")]
        public int Member_Dtg_Value { get; set; }

        [JsonPropertyName("dtg_adjust")]
        public string Dtg_Adjust { get; set; }

        [JsonPropertyName("submit_model")]
        public string Submit_Model { get; set; }

        [JsonPropertyName("fix_failed_model")]
        public string Fix_Failed_Model { get; set; }
    }
}
