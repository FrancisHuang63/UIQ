using System.Reflection;
using UIQ.Services.Interfaces;

namespace UIQ.Services
{
    public class UploadFileService : IUploadFileService
    {
        public bool DeleteUploadFile(string fileName)
        {
            var filePath = GetUploadPath(fileName);
            if (File.Exists(filePath) == false) return false;

            File.Delete(filePath);
            return true;
        }

        public string GetUploadPath(string fileName)
        {
#if DEBUG
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", fileName);
#endif
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "wwwroot", "upload", fileName);
        }

        public string GetUploadPathUrl(string fileName)
        {
            return string.IsNullOrWhiteSpace(fileName) ? string.Empty : $"/upload/{fileName.Replace("/", string.Empty)}";
        }

        public async Task UploadFileAsync(IFormFile file)
        {
            if (file == null || file?.Length == 0) return;

            var filePath = GetUploadPath(file.FileName);

            var checkDirPath = Path.GetDirectoryName(filePath);
            var checkFileName = Path.GetFileName(filePath);
            var checkDir = new DirectoryInfo(checkDirPath);

            using (var fileStream = new FileStream(Path.Combine(checkDirPath, file.Name), FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fileStream);
            }
        }
    }
}