﻿using System.Diagnostics;

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
                    FileName = command,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.StandardOutput.ReadLineAsync();
            process.WaitForExit();

            return output;
        }
    }
}