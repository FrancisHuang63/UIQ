using System.Text.Json.Serialization;

namespace UIQ.ViewModels
{
    public class ModelLogFileViewModel
    {
        [JsonPropertyName("model_name")]
        public string Model_Name { get; set; }

        [JsonPropertyName("model_id")]
        public int Model_Id { get; set; }

        [JsonPropertyName("member_name")]
        public string Member_Name { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("member_dtg_value")]
        public int Member_Dtg_Value { get; set; }

        [JsonPropertyName("dtg_adjust")]
        public string Dtg_Adjust { get; set; }

        [JsonPropertyName("submit_model")]
        public string Submit_Model { get; set; }
    }
}
