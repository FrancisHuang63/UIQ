using UIQ.Models;
using UIQ.ViewModels;

namespace UIQ.Services.Interfaces
{
    public interface IUiqService
    {
        public IEnumerable<HomeTableViewModel> GetHomeTableDatas();

        public Task<string> RunCommandAsync(string command);
        
        public void SaveCronSetting(string memberId, string cronMode);
        
        public Task<string> GetExecuteNwpRunningNodesCommandHtmlAsync(string selNode);

        public IEnumerable<ModelLogFileViewModel> GetModelLogFileViewModels();

        public Task<string> GetFullPathAsync(string modelName, string memberName, string nickname);
        
        public Task<IEnumerable<Model>> GetModelsAsync();

        public IEnumerable<ModelTimeViewModel> GetModelTimeDatas(string modelName, string memberName, string nickname, int startIndex, int pageSize, out int totalCount);
        
        public IEnumerable<BatchDetailViewModel> GetBatchDetailDatas(BatchDetailViewModelSearchParameter param);

        public IEnumerable<ShellDetailViewModel> GetShellDetailDatas(ShellDetailViewModelSearchParameter param);
    }
}
