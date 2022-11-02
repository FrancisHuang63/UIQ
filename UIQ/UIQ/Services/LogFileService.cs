using System.Reflection;
using System.Security.Claims;
using UIQ.Services.Interfaces;

namespace UIQ.Services
{
    public class LogFileService : ILogFileService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ClaimsPrincipal _currentUser;
        private string _LogDirectoryPath => $"{RootPath}/logfile";

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

        public LogFileService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _currentUser = _httpContextAccessor.HttpContext?.User;
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

        public async Task WriteUiActionLogFileAsync(string message)
        {
            var userAccount = _currentUser?.Identity?.Name ?? string.Empty;
            var fileFullPath = Path.Combine(_LogDirectoryPath, $"UI_actions_{DateTime.Now.ToString("yyyyMMdd")}.log");
            message = $"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}][user: {userAccount}] {message}";
            await WriteDataIntoLogFileAsync(_LogDirectoryPath, fileFullPath, message);
        }

        public async Task WriteUiErrorLogFileAsync(string message)
        {
            var userAccount = _currentUser?.Identity?.Name ?? string.Empty;
            var fileFullPath = Path.Combine(_LogDirectoryPath, $"UI_error_{DateTime.Now.ToString("yyyyMMdd")}.log");
            message = $"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}][user: {userAccount}] {message}";
            await WriteDataIntoLogFileAsync(_LogDirectoryPath, fileFullPath, message);
        }
    }
}