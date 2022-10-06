using UIQ.Models;
using UIQ.ViewModels;

namespace UIQ.Services.Interfaces
{
    public interface IUiqService
    {
        public IEnumerable<HomeTableViewModel> GetHomeTableDatas();

        public Task<string> RunCommandAsync(string command);

        public void UpdateCrontabMasterGroup(string cronMode);

        public Task<string> GetExecuteNwpRunningNodesCommandHtmlAsync(string selNode);

        public IEnumerable<ModelLogFileViewModel> GetModelLogFileViewModels();

        public Task<string> GetFullPathAsync(string modelName, string memberName, string nickname);

        public Task<IEnumerable<Model>> GetModelItemsAsync();

        public IEnumerable<ModelTimeViewModel> GetModelTimeDatas(string modelName, string memberName, string nickname, int startIndex, int pageSize, out int totalCount);

        public IEnumerable<BatchDetailViewModel> GetBatchDetailDatas(BatchDetailViewModelSearchParameter param);

        public IEnumerable<ShellDetailViewModel> GetShellDetailDatas(ShellDetailViewModelSearchParameter param);

        public IEnumerable<string> GetMemberRelay(string modelName, string memberName, string nickname);

        public IEnumerable<ArchiveViewModel> GetArchiveViewModels();

        public IEnumerable<string> GetArchiveDataTypes(string modelName, string memberName, string nickname);

        public Task<bool> UpsertCommandAsync(Command data);

        public Task<IEnumerable<string>> GetModelMemberPathAsync(string modelName, string memberName, string nickname);

        public Task<bool> DeleteCommandAsync(int commandId);

        public Task<IEnumerable<Command>> GetCommandItemsAsync();

        public Task<Command> GetCommandItemAsync(int commandId);

        public IEnumerable<CronSettingViewModel> GetCronSettingViewModels();

        public void UpdateGroupValidationWhoHasCronMode(string cronMode);

        public void UpdateGroupValidationWhoNotHasCronMode(string cronMode);

        public Task<IEnumerable<CronTab>> GetCronTabItemsAsync(int memberId);

        public Task<IEnumerable<Batch>> GetBatchItemsAsync(int memberId);

        public Task<IEnumerable<Archive>> GetArchiveItemsAsync(int memberId);

        public Task<IEnumerable<Output>> GetOutputItemsAsync(int memberId);

        public Task<IEnumerable<Data>> GetDataItemsAsync();

        public Task<IEnumerable<Work>> GetWorkItemsAsync();

        public Task<Member> GetMemberItemAsync(int memberId);

        public Task<IEnumerable<UploadFile>> GetUploadFileItemsAsync();
    }
}