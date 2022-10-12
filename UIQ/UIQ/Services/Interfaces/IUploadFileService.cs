namespace UIQ.Services.Interfaces
{
    public interface IUploadFileService
    {
        public Task UploadFileAsync(IFormFile file);

        public string GetUploadPathUrl(string fileName);

        public string GetUploadPath(string fileName);

        public bool DeleteUploadFile(string fileName);
    }
}