namespace UIQ.Services.Interfaces
{
    public interface ISshCommandService
    {
        public Task<string> RunCommand(string command);
    }
}
