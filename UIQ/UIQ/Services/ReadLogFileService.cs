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
    }
}