using System.Diagnostics;

namespace UIQ.Services.Interfaces
{
    public class SshCommandService : ISshCommandService
    {
        public async Task<string> RunCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return null;

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            await process.StandardInput.WriteLineAsync(command);
            var output = await process.StandardOutput.ReadLineAsync();

            return output;
        }
    }
}