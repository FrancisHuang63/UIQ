using UIQ.ViewModels;

namespace UIQ.Services.Interfaces
{
    public interface IUserService
    {
        public Task<bool> Login(string userId, string password);

        public Task Logout();
    }
}