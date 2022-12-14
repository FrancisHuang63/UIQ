using UIQ.Models;
using UIQ.ViewModels;

namespace UIQ.Services.Interfaces
{
    public interface IUiqService
    {
        public IEnumerable<HomeTableViewModel> GetHomeTableDatas();

        public Task<string> RunCommandAsync(string command);

        public void UpdateCrontabMasterGroup(string cronMode);

        public Task<ApiResponse<List<KeyValuePair<string, string>>>> GetExecuteNwpRunningNodesCommandResponseAsync(string selNode);

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

        public Task<CommandViewModel> GetCItemAsync(int commandId);

        public Task<string> GetCommandPwdAsync(int commandId);

        public Task<string> GetCommandContentAsync(int commandId);

        public Task<string> GetCommandExampleAsync(int commandId);

        public Task<IEnumerable<MenuViewModel>> GetMenuItemsWithPermissonAsync();

        public IEnumerable<CronSettingViewModel> GetCronSettingViewModels();

        public void UpdateGroupValidationWhoHasCronMode(string cronMode);

        public void UpdateGroupValidationWhoNotHasCronMode(string cronMode);

        public Task<IEnumerable<CronTab>> GetCronTabItemsAsync(int memberId);

        public Task<IEnumerable<Batch>> GetBatchItemsAsync(int memberId);

        public Task<IEnumerable<Archive>> GetArchiveItemsAsync(int memberId);

        public Task<IEnumerable<Output>> GetOutputItemsAsync(int memberId);

        public Task<IEnumerable<Data>> GetDataItemsAsync();

        public Task<IEnumerable<Work>> GetWorkItemsAsync();

        public Task<IEnumerable<GetShellDelayDataViewModel>> GetDelayDatasAsync(string userGroupName);

        public Task<Member> GetMemberItemAsync(int memberId);

        public Task<Member> GetMemberItemAsync(string modelName, string memberName, string nickname);

        public Task<string> GetMemberResetModelAsync(string modelName, string memberName, string nickname);

        public Task<int> DeleteDelayDataAsync(int id);

        public Task<bool> DeleteModelAsync(int modelId);

        public Task<bool> DeleteMemberAsync(int memberId);

        public Task<string> CheckRejectStatusAsync();

        public Task<ApiResponse<string>> SaveModelMemberSetData(ModelMemberSetSaveDataViewModel data);

        public IEnumerable<UploadFile> GetUploadFilePageItems(int startIndex, int pageSize, bool isUnPermisson, out int totalCount);

        public Task<string> DeleteRejectLogAsync();

        public Task<IEnumerable<Role>> GetRoleItemsAsync();

        public Task<Role> GetRoleItemAsync(int roleId);

        public Task<bool> SetUploadFileItems(IEnumerable<UploadFile> uploadFileDatas, IEnumerable<int> roleIds);

        public Task<IEnumerable<MenuRoleSetViewModel>> GetMenuRoleSetItemsAsync(int? roleId);

        public bool AddNewRole(string roleName, out int newRoleId);

        public Task<bool> UpdateRoleAsync(int roleId, string roleName);

        public Task<bool> UpdateMenuToRole(int roleId, int[] menuIds);

        public Task<IEnumerable<UserRoleSetViewModel>> GetUserRoleSetItemsAsync(int roleId);

        public Task<bool> UpdateUserToRole(int roleId, int[] userIds);

        public Task<Parameter> GetParameterItemAsync();

        public Task<bool> UpdateParameterAsync(Parameter data);

        public Task<bool> DeleteUploadFile(int fileId);

        public Task SqlSync();

        public Task<string> GetArchiveExecuteShellAsync(string modelName, string memberName, string nickname, string method);

        public Task<string> GetArchiveResultPathAsync(string modelName, string memberName, string nickname, string method);

        public Task<int> GetDataIdAsync(string method);

        public Task<IEnumerable<CheckPointInfoResultViewModel>> GetShell(CheckPointInfoViewModel data);

        public Task<IEnumerable<CheckPointInfoResultViewModel>> GetUnselectedShell(CheckPointInfoViewModel data);

        public Task<IEnumerable<ShowCheckPointInfoViewModel>> GetShowCheckPointInfoDatas(int memberId);

        public Task<IEnumerable<CheckPoint>> GetCheckPointsItemsAsync(int memberId);
    }
}