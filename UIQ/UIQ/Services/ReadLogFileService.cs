using UIQ.Services.Interfaces;

namespace UIQ.Services
{
    public class ReadLogFileService : IReadLogFileService
    {
        public string RootPath => $"{Directory.GetCurrentDirectory()}/wwwroot";

        public async Task<string> ReadLogFileAsync(string filePath)
        {
            if (System.IO.File.Exists(filePath) == false) return null;

            var logContnet = await System.IO.File.ReadAllTextAsync(filePath);
            return logContnet;
        }

        public async Task WriteDataIntoLogFileAsync(string filePath, string newData)
        {
            if (System.IO.File.Exists(filePath) == false)
            {
                System.IO.File.Create(filePath);
            }

            var logContnet = await ReadLogFileAsync(filePath);
            logContnet += newData;
            System.IO.File.WriteAllText(filePath, logContnet);
        }
    }
}