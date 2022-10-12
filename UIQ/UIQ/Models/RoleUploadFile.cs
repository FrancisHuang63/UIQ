using System.Text.Json.Serialization;

namespace UIQ.Models
{
    public class RoleUploadFile
    {
        [JsonPropertyName("role_id")]
        public int Role_Id { get; set; }

        [JsonPropertyName("file_id")]
        public int File_Id { get; set; }

        public RoleUploadFile(int roleId, int fileId)
        {
            Role_Id = roleId;
            File_Id = fileId;
        }

        public RoleUploadFile()
        {
        }
    }
}
