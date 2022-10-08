using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class UploadFile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("file_id")]
        public int File_Id { get; set; }

        [JsonPropertyName("file_name")]
        public string File_Name { get; set; }

        [JsonPropertyName("file_path")]
        public string File_Path { get; set; }

        [JsonPropertyName("create_datetime")]
        public DateTime Create_DateTime { get; set; }

        public UploadFile(string fileName, string filePath)
        {
            File_Name = fileName;
            File_Path = filePath;
            Create_DateTime = DateTime.Now;
        }

        public UploadFile()
        {
        }
    }
}