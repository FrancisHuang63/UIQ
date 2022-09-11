using UIQ.Models;
using UIQ.ViewModels;

namespace UIQ.Services.Interfaces
{
    public interface IUiqService
    {
        public IEnumerable<Member> GetMembers();

        public IEnumerable<Model> GetModels();

        public IEnumerable<HomeTableViewModel> GetHomeTableDatas();
    }
}
