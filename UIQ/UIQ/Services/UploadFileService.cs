using UIQ.Services.Interfaces;

namespace UIQ.Services
{
    public class UploadFileService : IUploadFileService
    {
        public string GetUploadPathUrl(string fileName)
        {
            return string.IsNullOrWhiteSpace(fileName) ? string.Empty : $"/upload/{fileName}";
        }

        public async Task UploadFileAsync(IFormFile file)
        {
            if (file?.Length == 0) return;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", file.FileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fileStream);
            }
        }
    }
}