namespace UIQ.Services.Interfaces
{
    public interface IReadLogFileService
    {
        string RootPath { get; }

        public Task<string> ReadLogFileAsync(string filePath);

        public Task WriteDataIntoLogFileAsync(string directoryPath, string fullFilePath, string newData);
    }
}
