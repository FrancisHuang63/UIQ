using UIQ.Models;
using UIQ.ViewModels;

namespace UIQ.Services.Interfaces
{
    public interface IUiqService
    {
        public IEnumerable<HomeTableViewModel> GetHomeTableDatas();

        public Task<string> RunCommand(string command);
        
        public void SaveCronSetting(string memberId, string cronMode);
        
        public Task<string> GetExecuteNwpRunningNodesCommandHtml(string selNode);
    }
}
