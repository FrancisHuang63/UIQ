namespace UIQ.Services.Interfaces
{
    public interface IUploadFileService
    {
        public Task UploadFileAsync(IFormFile file);

        public string GetUploadPathUrl(string fileName);
    }
}