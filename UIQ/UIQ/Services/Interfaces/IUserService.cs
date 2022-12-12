namespace UIQ.Services.Interfaces
{
    public interface IUserService
    {
        public Task<bool> Login(string userId, string password);

        public Task Logout();

        public Task<bool> CheckPasswordCorrect(string password);

        public Task<bool> ChangePassword(string newPassword);
    }
}