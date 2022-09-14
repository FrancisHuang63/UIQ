using UIQ.Models;
using UIQ.ViewModels;

namespace UIQ.Services.Interfaces
{
    public interface IUiqService
    {
        public IEnumerable<HomeTableViewModel> GetHomeTableDatas();
    }
}
