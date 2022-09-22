using Renci.SshNet;

namespace UIQ.Services.Interfaces
{
    public class SshCommandService : ISshCommandService
    {
        public async Task<string> RunCommandAsync(string command)
        {
            if(string.IsNullOrWhiteSpace(command)) return null;

            //TDDO

            //var cmd = SSH.CreateCommand("apt update && apt upgrade -y");
            //var asynch = cmd.BeginExecute();
            //var reader = new StreamReader(cmd.OutputStream);

            //while (!asynch.IsCompleted)
            //{
            //    var result = reader.ReadToEnd();
            //    if (string.IsNullOrEmpty(result))
            //        continue;
            //    Console.Write(result);
            //}
            //cmd.EndExecute(asynch);

            return string.Empty;
        }
    }
}
