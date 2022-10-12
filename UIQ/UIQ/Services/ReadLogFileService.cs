using System.Reflection;
using UIQ.Services.Interfaces;

namespace UIQ.Services
{
    public class ReadLogFileService : IReadLogFileService
    {
        public string RootPath
        {
            get
            {
#if DEBUG
                return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
#endif
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "wwwroot");
            }
        }

        public async Task<string> ReadLogFileAsync(string filePath)
        {
            if (System.IO.File.Exists(filePath) == false) return null;

            var logContnet = await System.IO.File.ReadAllTextAsync(filePath);
            return logContnet;
        }

        public async Task WriteDataIntoLogFileAsync(string directoryPath, string fullFilePath, string newData)
        {
            if (System.IO.Directory.Exists(directoryPath) == false)
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            }

            if (System.IO.File.Exists(fullFilePath) == false)
            {
                using (var fs = File.Create(fullFilePath))
                {
                }
            }

            var logContnet = await ReadLogFileAsync(fullFilePath);
            logContnet += newData;
            System.IO.File.WriteAllText(fullFilePath, logContnet);
        }
    }
}