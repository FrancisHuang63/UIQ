namespace UIQ.Services.Interfaces
{
    public interface ISshCommandService
    {
        public Task<string> RunCommandAsync(string command);
    }
}
