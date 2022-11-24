using Microsoft.Extensions.Options;
using System.Data;
using UIQ.Enums;
using UIQ.Models;
using UIQ.Services.Interfaces;
using UIQ.ViewModels;

namespace UIQ.Services
{
    public class UiqService : IUiqService
    {
        private readonly IDataBaseService _dataBaseNcsUiService;
        private readonly IDataBaseService _dataBaseNcsLogService;
        private readonly IUploadFileService _uploadFileService;
        private readonly ISshCommandService _sshCommandService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogFileService _logFileService;
        private readonly string _loginIp;
        private string _HpcCtl { get; set; }
        private string _SystemName { get; set; }
        private string _SystemDirectoryName { get; set; }
        private string _RshAccount { get; set; }
        private string _UiPath { get; set; }
        private string _HostName => System.Net.Dns.GetHostName();

        public UiqService(IHttpContextAccessor httpContextAccessor, IEnumerable<IDataBaseService> dataBaseServices
            , ISshCommandService sshCommandService, IConfiguration configuration
            , IUploadFileService uploadFileService, ILogFileService logFileService
            , IOptions<RunningJobInfoOption> runningJobInfoOption)
        {
            _dataBaseNcsUiService = dataBaseServices.Single(x => x.DataBase == Enums.DataBaseEnum.NcsUi);
            _dataBaseNcsLogService = dataBaseServices.Single(x => x.DataBase == Enums.DataBaseEnum.NcsLog);
            _uploadFileService = uploadFileService;
            _sshCommandService = sshCommandService;
            _httpContextAccessor = httpContextAccessor;
            _logFileService = logFileService;
            _HpcCtl = configuration.GetValue<string>("HpcCTL");
            _SystemName = configuration.GetValue<string>("SystemName");
            _SystemDirectoryName = configuration.GetValue<string>("SystemDirectoryName");
            _RshAccount = configuration.GetValue<string>("RshAccount");
            _UiPath = configuration.GetValue<string>("UiPath");

            var hostName = System.Net.Dns.GetHostName();
            var runningJobInfo = runningJobInfoOption.Value?.GetRunningJobInfo(hostName);
            _loginIp = runningJobInfo?.Items?.FirstOrDefault()?.Datas.FirstOrDefault()?.LoginIp;
        }

        public IEnumerable<HomeTableViewModel> GetHomeTableDatas()
        {
            var command = $"cat /{_SystemName}/{_HpcCtl}/shfun/shetc/Checkpoint_Lid |" + " awk '{print $1}'";
            var checkPointLid = RunCommandAsync(command).GetAwaiter().GetResult();

            var batchInfos = GetShowBatchInfos();
            var cronInfos = GetShowCronInfos();
            var modelConfigs = GetShowModelConfigs();

            Parse(modelConfigs, cronInfos, checkPointLid);
            var homeTableViewDatas = modelConfigs.Select((modelConfig, index) =>
            {
                var itemIndex = index + 1;
                return new HomeTableViewModel(modelConfig)
                {
                    AlertFlag = (modelConfig.Status?.ToUpper() == "FAIL" && modelConfig.Lid != Enums.LidEnum.Zero && modelConfig.Comment != "Cancelled"),
                    Href = $"/Home/DetailedStatus?modelMemberNickname={(System.Web.HttpUtility.UrlEncode($"{modelConfig.Model_Name}_{modelConfig.Member_Name}_{modelConfig.Nickname}"))}",
                };
            }).OrderBy(x => x.Member_Position).ToList();

            return homeTableViewDatas;
        }

        public async Task<string> RunCommandAsync(string command)
        {
            return await _sshCommandService.RunCommandAsync(command);
        }

        public void UpdateCrontabMasterGroup(string cronMode)
        {
            var sql = "UPDATE `crontab` SET `master_group` = @CronMode";
            _dataBaseNcsUiService.ExecuteWithTransactionAsync(sql, new { CronMode = cronMode });
        }

        public void UpdateGroupValidationWhoHasCronMode(string cronMode)
        {
            var sql = @"UPDATE `crontab`
	                    SET `group_validation` = CASE WHEN `cron_group` = @Cronmode THEN 1 ELSE 0 END
	                    WHERE `member_id` IN (SELECT `member_id`
                                              FROM `crontab`
                                              WHERE `cron_group` = @Cronmode GROUP BY `member_id`)";
            _dataBaseNcsUiService.ExecuteWithTransactionAsync(sql, new { CronMode = cronMode });
        }

        public void UpdateGroupValidationWhoNotHasCronMode(string cronMode)
        {
            var sql = @"UPDATE `crontab`
	                    SET `group_validation` = CASE WHEN `cron_group` = 'Normal' THEN 1 ELSE 0 END
	                    WHERE `member_id` IN (SELECT `member_id`
                                              FROM `crontab`
                                              WHERE `cron_group` <> @Cronmode GROUP BY `member_id`)";
            _dataBaseNcsUiService.ExecuteWithTransactionAsync(sql, new { CronMode = cronMode });
        }

        public async Task<ApiResponse<List<KeyValuePair<string, string>>>> GetExecuteNwpRunningNodesCommandResponseAsync(string selNode)
        {
            if (string.IsNullOrWhiteSpace(selNode))
                return new ApiResponse<List<KeyValuePair<string, string>>>("Node is not exist");

            var datas = new List<KeyValuePair<string, string>>();
            foreach (var node in selNode.Split(','))
            {
                var command = $"sudo -u {_RshAccount} ssh -l {_RshAccount} {node} /{_SystemName}/{_HpcCtl}/web/shell/ps.ksh";
                var commandResult = await RunCommandAsync(command);
                datas.Add(new KeyValuePair<string, string>(node, commandResult));
            }

            return new ApiResponse<List<KeyValuePair<string, string>>>(datas);
        }

        public IEnumerable<ModelLogFileViewModel> GetModelLogFileViewModels()
        {
            var sql = @"SELECT `model`.`model_name` AS `md_name`,
                        `member`.`model_id` AS `md_id`,
                        `member`.`member_name` AS `mb_name`,
                        `member`.`nickname`,
                        `member`.`account` AS `acnt`,
                        `member`.`member_dtg_value`,
                        `member`.`dtg_adjust`,
                        `member`.`submit_model`,
                        `member`.`fix_failed_model`
                        FROM `member`
                        LEFT JOIN `model` ON `member`.`model_id` = `model`.`model_id`
                        ORDER BY model.model_position,member.member_position";

            return _dataBaseNcsUiService.QueryAsync<ModelLogFileViewModel>(sql).GetAwaiter().GetResult();
        }

        public async Task<string> GetFullPathAsync(string modelName, string memberName, string nickname)
        {
            var sql = @"SELECT `member`.`account` AS `acnt`, `member`.`member_path`
                        FROM `member`
                        LEFT JOIN `model` ON `member`.`model_id` = `model`.`model_id`
                        WHERE `model`.`model_name` = @modelName
                        AND `member`.`member_name` = @memberName
                        AND `member`.`nickname` = @nickname";

            var datas = await _dataBaseNcsUiService.QueryAsync<FullPathViewModel>(sql, new { modelName = modelName, memberName = memberName, nickname = nickname });

            var result = datas.FirstOrDefault();
            if (result == null) return string.Empty;

            var secureAcnt = result.Acnt.GetGetSecureString();
            return $"/{_SystemName}/{secureAcnt.GetSecureStringToString()}{result.Member_Path}/{modelName}/{memberName}";
        }

        public async Task<IEnumerable<Model>> GetModelItemsAsync()
        {
            var datas = await _dataBaseNcsUiService.GetAllAsync<Model>("model");
            return datas;
        }

        public IEnumerable<ModelTimeViewModel> GetModelTimeDatas(string modelName, string memberName, string nickname, int startIndex, int pageSize, out int totalCount)
        {
            var sql = @"SELECT SQL_CALC_FOUND_ROWS
	                        etr.`model`,
	                        etr.`member`,
	                        mb.`nickname`,
	                        CEILING(etr.`avg_execution_time` / 60) AS `avg_execution_time`,
	                        etr.`run_type`,
	                        etr.`cron_mode`,
	                        etr.`typhoon_mode`,
	                        etr.`round`,
                            (SELECT SUM(bt.`batch_time`) FROM `db_ncsUI`.`batch` bt
	                         WHERE bt.`member_id` = mb.`member_id`
                             AND (bt.`batch_type` = '' OR bt.`batch_type` = SUBSTRING_INDEX(etr.`run_type`, '_', 1))
                             AND (bt.`batch_dtg` = '' OR bt.`batch_dtg` = etr.`round`)
                             GROUP BY(bt.`member_id`)
	                        ) AS `batch_time`
                        FROM `db_ncs_log`.`execution_time_result` etr
                        JOIN `db_ncsUI`.`member` mb ON mb.`member_name` = etr.`member` AND mb.`account` = etr.`account`
                        JOIN `db_ncsUI`.`model` md ON md.`model_name` = etr.`model` AND md.`model_id` = mb.`model_id`
                        WHERE etr.`account` = mb.`account`
                        AND md.`model_id` = mb.`model_id`
                        AND etr.`batch_name` = @BatchName
                        AND etr.`shell_name` = @ShellName";
            if (_httpContextAccessor.HttpContext.User.IsInRole(GroupNameEnum.ADM.ToString())) sql += " AND mb.`maintainer_status` = 0";
            if (!string.IsNullOrWhiteSpace(modelName?.Replace("-", string.Empty))) sql += " AND etr.`model` = @ModelName";
            if (!string.IsNullOrWhiteSpace(memberName?.Replace("-", string.Empty))) sql += " AND etr.`member` = @MemberName";
            if (!string.IsNullOrWhiteSpace(nickname?.Replace("-", string.Empty))) sql += " AND mb.`nickname` = @Nickname";

            sql += " ORDER BY model_position, member_position, run_type,cron_mode, typhoon_mode, round ASC";
            sql += $" LIMIT {pageSize} OFFSET {startIndex}";

            var param = new
            {
                BatchName = "unknown",
                ShellName = "unknown",
                ModelName = modelName,
                MemberName = memberName,
                Nickname = nickname,
            };

            var result = _dataBaseNcsLogService.QueryAsync<ModelTimeViewModel>(sql, param).GetAwaiter().GetResult();
            totalCount = _dataBaseNcsLogService.QueryAsync<int>("SELECT FOUND_ROWS() as total").GetAwaiter().GetResult().FirstOrDefault();
            return result;
        }

        public IEnumerable<BatchDetailViewModel> GetBatchDetailDatas(BatchDetailViewModelSearchParameter param)
        {
            var sql = @"SELECT `batch_name`, `batch_time`, `batch_type`, `batch_dtg`
                        FROM `batch` bt
                        JOIN `member` mb ON mb.`member_id` = bt.`member_id`
                        JOIN `model` md ON md.`model_id` = mb.`model_id`
                        WHERE (bt.`batch_type` = '' OR bt.`batch_type` = SUBSTRING_INDEX(@RunType, '_', 1))
                        AND md.`model_name` = @ModelName
                        AND mb.`member_name` = @MemberName
                        AND mb.`nickname` = @NickName
                        AND (bt.`batch_dtg` = '' OR bt.`batch_dtg` = @Round)
                        ORDER BY batch_position ASC";

            var batchList = _dataBaseNcsUiService.QueryAsync<BatchListViewModel>(sql, param).GetAwaiter().GetResult();

            sql = @"SELECT CEILING(`avg_execution_time` / 60) AS `avg_execution_time`
                    FROM `execution_time_result`
                    WHERE `model` = @ModelName
                    AND `member` = @MemberName
                    AND `account` = (SELECT mb.`account`
                                        FROM `db_ncsUI`.`member` mb
                                        JOIN `db_ncsUI`.`model` md ON md.`model_id` = mb.`model_id`
                                        WHERE md.`model_name` = @ModelName
                                        AND mb.`member_name` = @MemberName
                                        AND mb.`nickname` = @Nickname)
                    AND `run_type` = @RunType
                    AND `round` = @Round
                    AND `typhoon_mode` = @TyphoonMode
                    AND `cron_mode` = @CronMode
                    AND `batch_name` = @BatchName
                    AND `shell_name` = 'unknown'";
            var tmpBatchTime = 0;
            var result = batchList.Select(batchItem =>
            {
                param.BatchName = batchItem.Batch_Name;
                tmpBatchTime += batchItem.Batch_Time;
                var avgExecutionTime = _dataBaseNcsLogService.QueryAsync<int?>(sql, param).GetAwaiter().GetResult().FirstOrDefault();

                return new BatchDetailViewModel
                {
                    Batch_Name = batchItem.Batch_Name,
                    Setting_Time = tmpBatchTime,
                    History_Time = avgExecutionTime.HasValue ? avgExecutionTime.Value.ToString() : "-",
                };
            }).ToList();

            return result;
        }

        public IEnumerable<ShellDetailViewModel> GetShellDetailDatas(ShellDetailViewModelSearchParameter param)
        {
            var sql = @"SELECT `batch_name`, `shell_name`, CEILING(`avg_execution_time` / 60) AS `avg_time_min`, `avg_execution_time` AS `avg_time_sec`
                        FROM `execution_time_result`
                        WHERE `model` = @ModelName
                        AND `member` = @MemberName
                        AND `account` = (SELECT mb.`account`
                                        FROM `db_ncsUI`.`member` mb
                                        JOIN `db_ncsUI`.`model` md ON md.`model_id` = mb.`model_id`
                                        WHERE md.`model_name` = @ModelName
                                        AND mb.`member_name` = @MemberName
                                        AND mb.`nickname` = @Nickname)
                        AND `typhoon_mode` = @TyphoonMode
                        AND `run_type` = @RunType
                        AND `cron_mode` = @CronMode
                        AND `round` = @Round
                        AND `batch_name` != 'unknown'
                        AND `shell_name` != 'unknown'
                        ORDER BY `avg_execution_time` ASC";

            var result = _dataBaseNcsLogService.QueryAsync<ShellDetailViewModel>(sql, param).GetAwaiter().GetResult();

            return result;
        }

        public IEnumerable<string> GetMemberRelay(string modelName, string memberName, string nickname)
        {
            var sql = @"SELECT `batch` FROM `batch_view`
                        WHERE `model` = @ModelName
                        AND `member` = @MemberName
                        AND `relay` = 1
                        GROUP BY `batch`";
            var param = new { ModelName = modelName, MemberName = memberName, Nickname = nickname };
            var result = _dataBaseNcsUiService.QueryAsync<string>(sql, param).GetAwaiter().GetResult();

            return result;
        }

        public IEnumerable<ArchiveViewModel> GetArchiveViewModels()
        {
            var sql = @"SELECT
                            `model`.`model_id` AS `md_id`, `model`.`model_name` AS `md_name`, `member`.`member_name` AS `mb_name`, `member`.`nickname`, `member`.`account` AS `acnt`
                        FROM (SELECT `archive`.`member_id`
                              FROM `archive`
                              GROUP BY `archive`.`member_id`
                        ) AS arch_info
                        LEFT JOIN `member` ON `member`.`member_id` = `arch_info`.`member_id`
                        LEFT JOIN `model` ON `model`.`model_id` = `member`.`model_id`";

            if (_httpContextAccessor.HttpContext.User.IsInRole(GroupNameEnum.ADM.ToString()))
                sql += " WHERE `member`.`maintainer_status` = '0'";

            sql += " ORDER BY `model`.`model_position`, `member`.`member_position`";
            var result = _dataBaseNcsUiService.QueryAsync<ArchiveViewModel>(sql).GetAwaiter().GetResult();

            return result;
        }

        public IEnumerable<string> GetArchiveDataTypes(string modelName, string memberName, string nickname)
        {
            var sql = @"SELECT `data`.`data_name`
                        FROM (SELECT `data_id`
	                          FROM `archive`
	                          WHERE `archive`.`member_id` = (SELECT `member`.`member_id`
								                             FROM `member`
								                             LEFT JOIN `model` ON `member`.`model_id` = `model`.`model_id`
								                             WHERE `model_name` = @ModelName
                                                             AND `member_name` = @MemberName
                                                             AND `nickname` = @Nickname)
	                         ) AS d_id
                        LEFT JOIN `data` ON `data`.`data_id` = d_id.`data_id`";
            var param = new { ModelName = modelName, MemberName = memberName, Nickname = nickname };
            var result = _dataBaseNcsUiService.QueryAsync<string>(sql, param).GetAwaiter().GetResult();

            return result;
        }

        public async Task<IEnumerable<string>> GetModelMemberPathAsync(string modelName, string memberName, string nickname)
        {
            var sql = @"SELECT `fix_failed_target_directory` FROM `model_member_view`
                        WHERE `model_name` = @ModelName
                        AND `member_name` = @MemberName
                        AND `nickname` = @Nickname";
            var param = new { ModelName = modelName, MemberName = memberName, Nickname = nickname };
            var result = await _dataBaseNcsUiService.QueryAsync<string>(sql, param);

            return result;
        }

        public async Task<IEnumerable<Command>> GetCommandItemsAsync()
        {
            return await _dataBaseNcsUiService.GetAllAsync<Command>("command");
        }

        public async Task<bool> UpsertCommandAsync(Command data)
        {
            data.Command_Pwd = data.Command_Pwd ?? string.Empty;
            data.Command_Desc = data.Command_Desc ?? string.Empty;
            data.Command_Content = data.Command_Content ?? string.Empty;
            data.Command_Example = data.Command_Example ?? string.Empty;

            if (data.Command_Id.HasValue)
                return await _dataBaseNcsUiService.UpdateAsync("command", data, new { Command_Id = data.Command_Id }) > 0;

            return await _dataBaseNcsUiService.InsertAsync("command", data) > 0;
        }

        public async Task<bool> DeleteCommandAsync(int commandId)
        {
            return await _dataBaseNcsUiService.DeleteAsync("command", new { Command_Id = commandId }) > 0;
        }

        public async Task<CommandViewModel> GetCItemAsync(int commandId)
        {
            var sql = @"SELECT `command_id` AS `c_id`
                              ,`command_name` AS `c_name`
                              ,`command_desc` AS `c_desc`
                              ,`command_content` AS `c_content`
                              ,`command_pwd` AS `c_pwd`
                              ,`execution_time`
                              ,`command_example` AS `c_example`
                        FROM `command` WHERE `command_id` = @CommandId";
            return (await _dataBaseNcsUiService.QueryAsync<CommandViewModel>(sql, new { CommandId = commandId })).FirstOrDefault();
        }

        public async Task<string> GetCommandPwdAsync(int commandId)
        {
            var sql = @"SELECT `command_pwd` FROM `command` WHERE `command_id` = @CommandId";
            return (await _dataBaseNcsUiService.QueryAsync<string>(sql, new { CommandId = commandId })).FirstOrDefault();
        }

        public async Task<string> GetCommandContentAsync(int commandId)
        {
            var sql = @"SELECT `command_content` FROM `command` WHERE `command_id` = @CommandId";
            return (await _dataBaseNcsUiService.QueryAsync<string>(sql, new { CommandId = commandId })).FirstOrDefault();
        }

        public async Task<string> GetCommandExampleAsync(int commandId)
        {
            var sql = @"SELECT `command_example` FROM `command` WHERE `command_id` = @CommandId";
            return (await _dataBaseNcsUiService.QueryAsync<string>(sql, new { CommandId = commandId })).FirstOrDefault();
        }

        public async Task<IEnumerable<MenuViewModel>> GetMenuItemsWithPermissonAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return new List<MenuViewModel>();

            var roleIds = (user.Claims.FirstOrDefault(x => x.Type == "RoleIds").Value ?? string.Empty).Split(",");
            var sql = @"SELECT * FROM `menu`
                        WHERE `menu_id` IN (SELECT `menu_id` FROM `role_menu` WHERE `role_id` IN @RoleIds)
                        ORDER BY `sort`";
            var result = await _dataBaseNcsUiService.QueryAsync<MenuViewModel>(sql, new { RoleIds = roleIds });
            return result;
        }

        public IEnumerable<CronSettingViewModel> GetCronSettingViewModels()
        {
            var sql = @"SELECT `cron_group`, CASE WHEN `cron_group` = (SELECT `master_group` FROM `crontab` GROUP BY `master_group` LIMIT 1) THEN 1
						                          ELSE 0
					                         END AS `is_master_group`
                        FROM `crontab` GROUP BY `cron_group`";

            return _dataBaseNcsUiService.QueryAsync<CronSettingViewModel>(sql).GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<CronTab>> GetCronTabItemsAsync(int memberId)
        {
            var result = await _dataBaseNcsUiService.GetAllAsync<CronTab>("crontab", new { member_id = memberId });
            return result;
        }

        public async Task<IEnumerable<Batch>> GetBatchItemsAsync(int memberId)
        {
            var result = await _dataBaseNcsUiService.GetAllAsync<Batch>("batch", new { member_id = memberId });
            return result;
        }

        public async Task<IEnumerable<Archive>> GetArchiveItemsAsync(int memberId)
        {
            var result = await _dataBaseNcsUiService.GetAllAsync<Archive>("archive", new { member_id = memberId });
            return result;
        }

        public async Task<IEnumerable<Output>> GetOutputItemsAsync(int memberId)
        {
            var result = await _dataBaseNcsUiService.GetAllAsync<Output>("output", new { member_id = memberId });
            return result;
        }

        public async Task<IEnumerable<Data>> GetDataItemsAsync()
        {
            var result = await _dataBaseNcsUiService.GetAllAsync<Data>("data");
            return result;
        }

        public async Task<IEnumerable<Work>> GetWorkItemsAsync()
        {
            var result = await _dataBaseNcsUiService.GetAllAsync<Work>("work");
            return result;
        }

        public async Task<IEnumerable<GetShellDelayDataViewModel>> GetDelayDatasAsync(string userGroupName)
        {
            var sql = $@"SELECT `check_point_delay`.`id`
							,`check_point_delay`.`predicted_end_time`
							,`check_point_delay`.`model_start_time`
							,`check_point_delay`.`run_type`
							,`check_point_delay`.`dtg`
							,`batch`.`batch_name`
							,`member`.`member_name`
							,`member`.`nickname`
							,`check_point`.`shell_name`
							,`model`.`model_name`
						FROM `check_point_delay`
						JOIN `check_point` ON `check_point_delay`.`check_id` = `check_point`.`check_id`
						JOIN `member` ON `check_point`.`member_id` = `member`.`member_id`
						JOIN `batch` ON `check_point`.`batch_id` = `batch`.`batch_id`
						JOIN `model` ON `member`.`model_id` = `model`.`model_id`
						WHERE `is_processed` = '0'
						{(_httpContextAccessor.HttpContext.User.IsInRole(GroupNameEnum.ADM.ToString()) == false
                            ? "AND `member`.`maintainer_status` = '0'"
                            : string.Empty)}";
            var result = await _dataBaseNcsUiService.QueryAsync<GetShellDelayDataViewModel>(sql);
            return result;
        }

        public async Task<Member> GetMemberItemAsync(int memberId)
        {
            var result = await _dataBaseNcsUiService.GetAllAsync<Member>("member", new { member_id = memberId });
            return result.FirstOrDefault();
        }

        public async Task<Member> GetMemberItemAsync(string modelName, string memberName, string nickname)
        {
            var sql = @"SELECT mb.*
                        FROM `member` mb
                        LEFT JOIN `model` md ON md.`model_id` = mb.`model_id`
                        WHERE md.`model_name` = @model_name
                        AND mb.`member_name` = @member_name
                        AND mb.`nickname` = @nickname";
            var result = await _dataBaseNcsUiService.QueryAsync<Member>(sql, new { model_name = modelName, member_name = memberName, nickname = nickname });
            return result.FirstOrDefault();
        }

        public async Task<string> GetMemberResetModelAsync(string modelName, string memberName, string nickname)
        {
            var sql = @"SELECT mb.`reset_model`
                        FROM `member` mb
                        LEFT JOIN `model` md ON md.`model_id` = mb.`model_id`
                        WHERE md.`model_name` = @model_name
                        AND mb.`member_name` = @member_name
                        AND mb.`nickname` = @nickname";
            var result = await _dataBaseNcsUiService.QueryAsync<string>(sql, new { model_name = modelName, member_name = memberName, nickname = nickname });
            return result.FirstOrDefault();
        }

        public async Task<int> DeleteDelayDataAsync(int id)
        {
            return await _dataBaseNcsUiService.UpdateAsync("check_point_delay", new { Is_Processed = "1" }, new { Id = id });
        }

        public async Task<bool> DeleteModelAsync(int modelId)
        {
            var result = await _dataBaseNcsUiService.DeleteAsync("model", new { Model_Id = modelId });
            return result > 0;
        }

        public async Task<bool> DeleteMemberAsync(int memberId)
        {
            var result = await _dataBaseNcsUiService.DeleteAsync("member", new { Member_Id = memberId });
            return result > 0;
        }

        public async Task<string> CheckRejectStatusAsync()
        {
            var filePath = $"{_UiPath}wwwroot/log/SMS/sms_start_reject.log";
            if (File.Exists(filePath) == false) return "status : normal";

            var content = await _logFileService.ReadLogFileAsync(filePath);
            content = content.Replace("\r\n", "\n").Replace("\r", "\n");
            return content.Trim();
        }

        public async Task<ApiResponse<string>> SaveModelMemberSetData(ModelMemberSetSaveDataViewModel data)
        {
            var ApiResult = new ApiResponse<string>();
            var result = 0;
            var memberId = data.Member.Member_Id;

            //Model
            if (data.IsNewModelName)
            {
                data.Model = new Model { Model_Name = data.New_Model_Name ?? string.Empty, Model_Position = data.New_Model_Position, };
                data.Model.Model_Id = (int)await _dataBaseNcsUiService.InsertAndReturnAutoGenerateIdAsync("model", data.Model);
            }

            //Member
            data.Member.Model_Id = data.Model.Model_Id;
            data.Member.Member_Name = data.Member.Member_Name ?? string.Empty;
            data.Member.Nickname = data.Member.Nickname ?? string.Empty;
            var secureAccount = (data.Member.Account ?? string.Empty).GetGetSecureString();
            data.Member.Account = secureAccount.GetSecureStringToString();

            var isMemberExist = await _dataBaseNcsUiService.IsExistAsync("member", new { Member_Id = data.Member.Member_Id });
            if (isMemberExist) result += await _dataBaseNcsUiService.UpdateAsync("member", data.Member, new { Member_Id = data.Member.Member_Id });
            else memberId = (int)await _dataBaseNcsUiService.InsertAndReturnAutoGenerateIdAsync("member", data.Member);

            data.Member.Member_Id = memberId;

            //CronTab
            result += await _dataBaseNcsUiService.DeleteAsync("crontab", new { Member_Id = memberId });
            if (data.CronTabs?.Any() ?? false)
            {
                foreach (var cronTab in data.CronTabs)
                {
                    cronTab.Member_Id = memberId;
                    cronTab.Start_Time = cronTab.Start_Time ?? string.Empty;
                    cronTab.Cron_Group = cronTab.Cron_Group ?? string.Empty;
                    cronTab.Master_Group = cronTab.Master_Group ?? string.Empty;

                    var paramter = new
                    {
                        Member_Id = cronTab.Member_Id,
                        Start_Time = cronTab.Start_Time,
                        Cron_Group = cronTab.Cron_Group,
                    };
                    var isExist = await _dataBaseNcsUiService.IsExistAsync("crontab", paramter);
                    if (isExist) result += await _dataBaseNcsUiService.UpdateAsync("crontab", cronTab, paramter);
                    else result += await _dataBaseNcsUiService.InsertAsync("crontab", cronTab);
                }
            }

            //Batch
            result += await _dataBaseNcsUiService.DeleteAsync("batch", new { Member_Id = memberId });
            var newBatchIdPair = new Dictionary<int, int>();
            if (data.Batchs?.Any() ?? false)
            {
                foreach (var batch in data.Batchs)
                {
                    batch.Member_Id = memberId;
                    batch.Batch_Dtg = batch.Batch_Dtg ?? string.Empty;
                    batch.Batch_Type = batch.Batch_Type ?? string.Empty;
                    batch.Batch_Name = batch.Batch_Name ?? string.Empty;

                    var newBatchId = (int)await _dataBaseNcsUiService.InsertAndReturnAutoGenerateIdAsync("batch", batch);
                    if (newBatchId > 0)
                    {
                        newBatchIdPair.Add(batch.Batch_Id, newBatchId);
                        result += 1;
                    }
                }
            }

            //Archive
            result += await _dataBaseNcsUiService.DeleteAsync("archive", new { Member_Id = memberId });
            if (data.Archives?.Any() ?? false)
            {
                foreach (var archive in data.Archives)
                {
                    archive.Member_Id = memberId;
                    var paramter = new { Archive_Id = archive.Archive_Id };
                    var isExist = await _dataBaseNcsUiService.IsExistAsync("archive", paramter);
                    if (isExist) result += await _dataBaseNcsUiService.UpdateAsync("archive", archive, paramter);
                    else result += await _dataBaseNcsUiService.InsertAsync("archive", archive);
                }
            }

            //Output
            result += await _dataBaseNcsUiService.DeleteAsync("output", new { Member_Id = memberId });
            if (data.Outputs?.Any() ?? false)
            {
                foreach (var output in data.Outputs)
                {
                    output.Member_Id = memberId;
                    output.Model_Output = output.Model_Output ?? string.Empty;
                    var paramter = new { Output_Id = output.Output_Id };
                    var isExist = await _dataBaseNcsUiService.IsExistAsync("output", paramter);
                    if (isExist) result += await _dataBaseNcsUiService.UpdateAsync("output", output, paramter);
                    else result += await _dataBaseNcsUiService.InsertAsync("output", output);
                }
            }

            //CheckPoint
            result += await _dataBaseNcsUiService.DeleteAsync("check_point", new { Member_Id = memberId });
            if (data.CheckPoints?.Any() ?? false)
            {
                foreach (var checkPoint in data.CheckPoints)
                {
                    var param = new
                    {
                        Model = data.Model.Model_Name ?? string.Empty,
                        Member = data.Member.Member_Name ?? string.Empty,
                        Account = data.Member.Account ?? string.Empty,
                        Batch_Name = checkPoint.Batch_Name ?? string.Empty,
                        Shell_Name = checkPoint.Shell_Name ?? string.Empty,
                    };
                    var isExist = await _dataBaseNcsLogService.IsExistAsync("execution_time_result", param);
                    if (isExist == false)
                    {
                        var executionTimeResult = new ExecutionTimeResult()
                        {
                            Model = data.Model.Model_Name ?? string.Empty,
                            Member = data.Member.Member_Name ?? string.Empty,
                            Account = data.Member.Account ?? string.Empty,
                            Batch_Name = checkPoint.Batch_Name ?? string.Empty,
                            Shell_Name = checkPoint.Shell_Name ?? string.Empty,
                        };
                        await _dataBaseNcsLogService.InsertAsync("execution_time_result", executionTimeResult);
                    }

                    checkPoint.Member_Id = memberId;
                    checkPoint.Model_Id = data.Model.Model_Id;
                    var checkPointData = new CheckPoint()
                    {
                        Batch_Id = newBatchIdPair[checkPoint.Batch_Id],
                        Member_Id = checkPoint.Member_Id,
                        Shell_Name = checkPoint.Shell_Name ?? string.Empty,
                        Tolerance_Time = checkPoint.Tolerance_Time,
                    };
                    result += await _dataBaseNcsUiService.InsertAsync("check_point", checkPointData);
                }
            }
            if (result > 0)
                ApiResult.Success = true;
            else
                ApiResult.Success = false;

            ApiResult.Data = memberId.ToString();

            return ApiResult;
        }

        public IEnumerable<UploadFile> GetUploadFilePageItems(int startIndex, int pageSize, bool isUnPermisson, out int totalCount)
        {
            var sql = @$"SELECT SQL_CALC_FOUND_ROWS *
                         FROM `upload_file`
                         {(isUnPermisson ? "" : @$"WHERE `file_id` IN (SELECT `file_id` FROM `role_upload_file` WHERE `role_id` IN (SELECT `role_id`
                                                                                                          FROM `role_user`
                                                                                                          WHERE `user_id` = @UserId)
                                                                                      OR `role_id` = -1)")}
                         ORDER BY `create_datetime` DESC
                         LIMIT {pageSize} OFFSET {startIndex}";

            var userId = _httpContextAccessor?.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id").Value ?? string.Empty;

            var result = _dataBaseNcsUiService.QueryAsync<UploadFile>(sql, new { UserId = userId }).GetAwaiter().GetResult();
            totalCount = _dataBaseNcsUiService.QueryAsync<int>("SELECT FOUND_ROWS() as total").GetAwaiter().GetResult().FirstOrDefault();
            return result;
        }

        public async Task<string> DeleteRejectLogAsync()
        {
            var file_path = $"{_UiPath}wwwroot/log/SMS/sms_start_reject.log";
            var message = $"{file_path} does not exist!";

            if (File.Exists(file_path) == false) return message;

            var rtn_code = await RunCommandAsync($"sudo -u {_RshAccount} ssh {_loginIp} -l {_HpcCtl} {_UiPath}wwwroot/shell/sms_reject_clean.sh;echo $?");
            return "status: " + rtn_code;
        }

        public async Task<bool> SetUploadFileItems(IEnumerable<UploadFile> uploadFileDatas, IEnumerable<int> roleIds)
        {
            var result = 0;
            foreach (var uploadFileData in uploadFileDatas)
            {
                var newId = await _dataBaseNcsUiService.InsertAndReturnAutoGenerateIdAsync("upload_file", uploadFileData);
                result = await _dataBaseNcsUiService.InsertAsync("role_upload_file", roleIds.Select(x => new RoleUploadFile(x, (int)newId)).ToList());
            }
            return result > 0;
        }

        public async Task<IEnumerable<MenuRoleSetViewModel>> GetMenuRoleSetItemsAsync(int? roleId)
        {
            var sql = $@"SELECT m.*, (SELECT EXISTS(SELECT 1
                                                   FROM `role_menu`
                                                   WHERE `menu_id` = m.`menu_id` AND `role_id` = @RoleId))
                                      AS `is_selected`
                         FROM `menu` m
                         ORDER BY `sort`";
            var result = await _dataBaseNcsUiService.QueryAsync<MenuRoleSetViewModel>(sql, new { RoleId = roleId });
            return result;
        }

        public async Task<IEnumerable<Role>> GetRoleItemsAsync()
        {
            return await _dataBaseNcsUiService.GetAllAsync<Role>("role");
        }

        public async Task<Role> GetRoleItemAsync(int roleId)
        {
            var result = await _dataBaseNcsUiService.GetAllAsync<Role>("role", new { role_id = roleId });
            return result.FirstOrDefault();
        }

        public bool AddNewRole(string roleName, out int newRoleId)
        {
            var role = new Role(roleName);
            newRoleId = (int)_dataBaseNcsUiService.InsertAndReturnAutoGenerateIdAsync("role", role).GetAwaiter().GetResult();
            return newRoleId > 0;
        }

        public async Task<bool> UpdateRoleAsync(int roleId, string roleName)
        {
            var sql = @"UPDATE `role`
                        SET `role_name` = @RoleName,
                            `last_update_datetime` = @LastUpdateDatetime
                        WHERE `role_id` = @RoleId";
            var param = new
            {
                RoleId = roleId,
                RoleName = roleName,
                LastUpdateDatetime = DateTime.Now,
            };
            var result = await _dataBaseNcsUiService.ExecuteWithTransactionAsync(sql, param);
            return result > 0;
        }

        public async Task<bool> UpdateMenuToRole(int roleId, int[] menuIds)
        {
            var result = 0;
            result += await _dataBaseNcsUiService.DeleteAsync("role_menu", new { role_id = roleId });

            var datas = menuIds?.Select(menuId => new RoleMenu(roleId, menuId)).ToList();
            result += await _dataBaseNcsUiService.InsertAsync("role_menu", datas);

            return result > 0;
        }

        public async Task<IEnumerable<UserRoleSetViewModel>> GetUserRoleSetItemsAsync(int roleId)
        {
            var sql = $@"SELECT u.*, (SELECT EXISTS(SELECT 1
                                                   FROM `role_user`
                                                   WHERE `user_id` = u.`user_id` AND `role_id` = @RoleId))
                                      AS `is_selected`
                         FROM `user` u";
            var result = await _dataBaseNcsUiService.QueryAsync<UserRoleSetViewModel>(sql, new { RoleId = roleId });
            return result;
        }

        public async Task<bool> UpdateUserToRole(int roleId, int[] userIds)
        {
            var result = 0;
            result += await _dataBaseNcsUiService.DeleteAsync("role_user", new { role_id = roleId });

            var datas = userIds?.Select(userId => new RoleUser(roleId, userId)).ToList();
            result += await _dataBaseNcsUiService.InsertAsync("role_user", datas);

            return result > 0;
        }

        public async Task<Parameter> GetParameterItemAsync()
        {
            var sql = @"SELECT * FROM `parameter` WHERE `parameter_id` = @ParameterId";
            return (await _dataBaseNcsUiService.QueryAsync<Parameter>(sql, new { ParameterId = "HomeFreshFrequency" })).FirstOrDefault();
        }

        public async Task<bool> UpdateParameterAsync(Parameter data)
        {
            return await _dataBaseNcsUiService.UpdateAsync("parameter", data, new { parameter_id = data.Parameter_Id }) > 0;
        }

        public async Task<bool> DeleteUploadFile(int fileId)
        {
            var uploadFile = await GetUploadFileItemAsync(fileId);
            if (uploadFile == null) return false;

            var sql = @"DELETE FROM `role_upload_file` WHERE `file_id` = @FileId;
                        DELETE FROM `upload_file` WHERE `file_id` = @FileId";

            var result = (await _dataBaseNcsUiService.ExecuteWithTransactionAsync(sql, new { FileId = fileId })) > 0;
            _uploadFileService.DeleteUploadFile(uploadFile.File_Name);
            return result;
        }

        public async Task SqlSync()
        {
            var hpcSql = _dataBaseNcsUiService.DataBaseName;
            var account = _dataBaseNcsUiService.DataBaseUid;
            var password = _dataBaseNcsUiService.DataBasePwd;
            var baseDir = $"/{_SystemName}/{_HpcCtl}/web";
            var dateString = DateTime.Now.ToString("yyMMdd");
            var filename = $"{baseDir}/{hpcSql}{dateString}.sql";
            var myHost = _HostName.Trim();
            var toHost = string.Empty;
            switch (myHost)
            {
                case "login13":
                    toHost = "login14";
                    break;

                case "login14":
                    toHost = "login13";
                    break;

                case "datamv13":
                    toHost = "datamv14";
                    break;

                case "datamv14":
                    toHost = "datamv13";
                    break;

                case "h6dm13":
                    toHost = "h6dm14";
                    break;

                case "h6dm14":
                    toHost = "h6dm13";
                    break;

                case "login21":
                    toHost = "login22";
                    break;

                case "login22":
                    toHost = "login21";
                    break;

                case "datamv21":
                    toHost = "datamv22";
                    break;

                case "datamv22":
                    toHost = "datamv21";
                    break;

                case "h6dm21":
                    toHost = "h6dm22";
                    break;

                case "h6dm22":
                    toHost = "h6dm21";
                    break;

                default:
                    break;
            }

            EditDump(filename);

            // copy to TOHOST
            if (!string.IsNullOrEmpty(toHost))
                await RunCommandAsync($"sudo -u {_RshAccount} ssh -l {_HpcCtl} {toHost} mysql -u{account} -p{password} --default-character-set=utf8 {hpcSql} < {filename}");
        }

        public async Task<string> GetArchiveExecuteShellAsync(string modelName, string memberName, string nickname, string method)
        {
            var sql = @"SELECT `archive_redo`
						FROM `archive_view`
					    WHERE `model_name` = @ModelName
						AND `member_name`= @MemberName
						AND `data_name` = @DataName";
            var param = new { ModelName = modelName, MemberName = memberName, DataName = method };
            var result = await _dataBaseNcsUiService.QueryAsync<string>(sql, param);
            return result.FirstOrDefault();
        }

        public async Task<string> GetArchiveResultPathAsync(string modelName, string memberName, string nickname, string method)
        {
            var sql = @"SELECT `target_directory`
						FROM `archive_view`
					    WHERE `model_name` = @ModelName
						AND `member_name`= @MemberName
						AND `data_name` = @DataName";
            var param = new { ModelName = modelName, MemberName = memberName, Nickname = nickname, DataName = method };
            var result = await _dataBaseNcsUiService.QueryAsync<string>(sql, param);
            return result.FirstOrDefault();
        }

        public async Task<int> GetDataIdAsync(string method)
        {
            var sql = @"SELECT `data_id` FROM `data` WHERE `data_name` = @DataName";
            var param = new { DataName = method };
            var result = await _dataBaseNcsUiService.QueryAsync<int>(sql, param);
            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<CheckPointInfoResultViewModel>> GetShell(CheckPointInfoViewModel data)
        {
            var sql = "SELECT `model_name` FROM `model` WHERE `model_id` = @Model_Id";
            data.Md_Name = (await _dataBaseNcsUiService.QueryAsync<string>(sql, new { Model_Id = data.Md_Id })).FirstOrDefault();

            sql = @"SELECT `shell_name`, `round`, `typhoon_mode`, `avg_execution_time`
                    FROM `execution_time_result`
                    WHERE `member` = @Member_Name
                    AND `account` = @Member_Account
                    AND `batch_name` = @Batch_Name
                    AND `model` = @Model_Name
                    AND `cron_mode` = @Cron_Mode
                    AND `shell_name` != 'unknown'";

            var whereSql = new List<string>();
            if (string.IsNullOrWhiteSpace(data.Run_Type) == false) whereSql.Add("`run_type` LIKE CONCAT(@Run_Type, '%')");
            if (string.IsNullOrWhiteSpace(data.Round) == false) whereSql.Add("`round` = @Round");

            sql += whereSql.Any() ? string.Join(" AND ", whereSql) : string.Empty;
            var result = await _dataBaseNcsLogService.QueryAsync<CheckPointInfoResultViewModel>(sql, data);
            return result;
        }

        public async Task<IEnumerable<CheckPointInfoResultViewModel>> GetUnselectedShell(CheckPointInfoViewModel data)
        {
            var sql = @"SELECT `model_name` FROM `model`
                        WHERE `model_id` = @Model_Id";
            var modelName = (await _dataBaseNcsUiService.QueryAsync<string>(sql, new { Model_Id = data.Md_Id })).FirstOrDefault();
            data.Md_Name = modelName;

            sql = $@"SELECT `shell_name` AS `sh_name`, `round`, `typhoon_mode`, `avg_execution_time`
                     FROM `execution_time_result`
                     WHERE `member` = @Member_Name
                     AND `account` = @Member_Account
                     AND `batch_name` = @Batch_Name
                     AND `model` = @Model_Name
                     AND `cron_mode` = @Cron_Mode
                     AND `cron_mode` = 'Backup'
                     AND `shell_name` != @Shell_Name
                     AND `shell_name` != 'unknown'
                     {(string.IsNullOrWhiteSpace(data.Round) == false ? " AND `round` = @Round" : string.Empty)}
                     {(string.IsNullOrWhiteSpace(data.Run_Type) ? string.Empty : "AND `run_type` LIKE CONCAT(@Run_Type, '%')")}";

            var result = await _dataBaseNcsLogService.QueryAsync<CheckPointInfoResultViewModel>(sql, data);
            return result;
        }

        public async Task<IEnumerable<ShowCheckPointInfoViewModel>> GetShowCheckPointInfoDatas(int memberId)
        {
            var result = new List<ShowCheckPointInfoViewModel>();
            var sql = @"SELECT model.model_name AS `md_name`
                             , `member`.`member_name` AS `mb_name`
                             , `member`.`nickname`
                             , `member`.`account` AS `acnt`
                             , `batch`.`batch_id`
                             , `batch`.`batch_name`
                             , `batch`.`batch_type`
                             , `batch`.`batch_dtg`
                             , `check_point`.`shell_name`
                             , `check_point`.`tolerance_time`
                        FROM `check_point`
                        JOIN `batch` ON `batch`.`batch_id` = `check_point`.`batch_id`
                        JOIN `member` ON `member`.`member_id` = `check_point`.`member_id`
                        JOIN `model` ON `model`.`model_id` = `member`.`model_id`
                        WHERE `check_point`.`member_id` = @MemberId
                        ORDER BY `check_point`.`batch_id` ASC, `check_point`.`check_id` ASC";

            var memberCheckPoints = await _dataBaseNcsUiService.QueryAsync<MemberCheckPoint>(sql, new { MemberId = memberId });
            foreach (var item in memberCheckPoints)
            {
                sql = $@"SELECT `typhoon_mode`, CEILING(avg(`avg_execution_time`)/60) AS `avg_execution_time`
                        FROM `execution_time_result`
                        WHERE `model` = @Model_Name
                        AND `member` = @Member_Name
                        AND `account` = @Account
                        AND `batch_name` = @Batch_Name
                        AND `shell_name` = @Shell_Name
                        {(string.IsNullOrWhiteSpace(item.Batch_Type) ? string.Empty : "AND `run_type` LIKE CONCAT(@Batch_Type, '%')")}
                        {(string.IsNullOrWhiteSpace(item.Batch_Dtg) ? string.Empty : "AND `round` = @Batch_Dtg")}
                        GROUP BY `typhoon_mode`";

                var avgTimes = await _dataBaseNcsLogService.QueryAsync<AvgTime>(sql, item);

                result.Add(new ShowCheckPointInfoViewModel(item)
                {
                    Avg_Time = avgTimes.ToList()
                });
            }

            return result;
        }

        public async Task<IEnumerable<CheckPoint>> GetCheckPointsItemsAsync(int memberId)
        {
            var result = await _dataBaseNcsUiService.GetAllAsync<CheckPoint>("check_point", new { member_id = memberId });
            return result;
        }

        #region Private Methods

        private void Parse(IEnumerable<ModelConfigViewModel> modelInfos, IEnumerable<CronInfoViewModel> cronInfos, string checkPointLid)
        {
            foreach (var modelInfo in modelInfos)
            {
                modelInfo.Member_Path = modelInfo.Member_Path ?? string.Empty;
                var maxPerRunTime = modelInfo.Member_Dtg_Value * 60 * 60;
                if (modelInfo.Status == "Cancelled" || modelInfo.Status?.ToUpper() == "FAIL")
                    WriteDebugMessage($"[{DateTime.Now.ToString("HH:mm:ss")}]${modelInfo.Model_Name}_${modelInfo.Member_Name}_${modelInfo.Nickname}(C) {modelInfo.Status}, run_end:{modelInfo.Run_End.ToString("HH:mm:ss")}\n");

                modelInfo.PreTime = GetPreTime(modelInfo.Model_Name, modelInfo.Member_Name, modelInfo.Nickname);

                #region consider delay 工作 啟始延遲判斷   (running)

                var preStartTime = new DateTime(modelInfo.Pre_Start.Ticks);
                var now = DateTime.Now;
                var workStartTime = new DateTime(modelInfo.Sms_Time?.Ticks ?? 0);

                //若工作起始時間 > 現在時間 表跨天
                if (workStartTime > now) workStartTime = workStartTime.AddDays(-1);

                //若預測起始時間 > 現在時間 表跨天 (時間差異應6小時以上)
                if (preStartTime > now && (preStartTime.GetUnixTimestamp() - now.GetUnixTimestamp()) > maxPerRunTime) preStartTime = preStartTime.AddDays(-1);

                //若預測起始時間 < 現在時間 表跨天 (時間差異應6小時以上)
                if (preStartTime < now && (now.GetUnixTimestamp() - preStartTime.GetUnixTimestamp()) > maxPerRunTime) preStartTime = preStartTime.AddDays(1);

                var diffMinutes = (workStartTime.GetUnixTimestamp() - preStartTime.GetUnixTimestamp()) / 60D;
                modelInfo.Comment = "on time";

                if (diffMinutes > 10)
                {
                    modelInfo.Comment = "delay 10+ mins";
                    WriteDebugMessage($"[{now.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(01) {modelInfo.Comment}, pre_start:{preStartTime.ToString("HH:mm:ss")}, job_started:{workStartTime.ToString("HH:mm:ss")}, diff:{diffMinutes}\n");
                }
                if (diffMinutes > 30)
                {
                    modelInfo.Comment = "delay 30+ mins";
                    WriteDebugMessage($"[{now.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(02) {modelInfo.Comment}, pre_start:{preStartTime.ToString("HH:mm:ss")}, job_started:{workStartTime.ToString("HH:mm:ss")}, diff:{diffMinutes}\n");
                }
                if (diffMinutes > 60)
                {
                    modelInfo.Comment = "delay 1hr+";
                    WriteDebugMessage($"[{now.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(03) {modelInfo.Comment}, pre_start:{preStartTime.ToString("HH:mm:ss")}, job_started:{workStartTime.ToString("HH:mm:ss")}, diff:{diffMinutes}\n");
                }
                if (modelInfo.Sms_Time == null)
                {
                    modelInfo.Comment = "unknown";
                    WriteDebugMessage($"[{now.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(04) {modelInfo.Comment}, pre_start:{preStartTime.ToString("HH:mm:ss")}, job_started:{workStartTime.ToString("HH:mm:ss")}, diff:{diffMinutes}\n");
                }

                #endregion consider delay 工作 啟始延遲判斷   (running)

                #region 工作結束延遲   (running)

                var end1 = new DateTime(modelInfo.Pre_End.Ticks);  //預測結束時間
                if (modelInfo.Status == "RUNNING")
                {
                    var start3 = new DateTime(modelInfo.Start_Time.Ticks);    //工作起始時間
                    if (now < start3)
                    {
                        start3 = start3.AddDays(-2);
                        end1 = (start3 < end1 && (end1.GetUnixTimestamp() - start3.GetUnixTimestamp()) > maxPerRunTime)
                            ? end1.AddDays(-2)
                            : end1;
                    }
                    else
                    {
                        end1 = start3 > end1 ? end1.AddDays(2) : end1;
                    }

                    diffMinutes = (now.GetUnixTimestamp() - end1.GetUnixTimestamp()) / 60D;
                    if (modelInfo.Comment == "on time" && now > end1)
                    {
                        if (diffMinutes > 10)
                        {
                            modelInfo.Comment = "delay 10+ mins";
                            WriteDebugMessage($"[{now.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(05) {modelInfo.Comment}, pre_end:{end1.ToString("HH:mm:ss")}, job_started:{start3.ToString("HH:mm:ss")}, diff:{diffMinutes}\n");
                        }
                        if (diffMinutes > 30)
                        {
                            modelInfo.Comment = "delay 30+ mins";
                            WriteDebugMessage($"[{now.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(06) {modelInfo.Comment}, pre_end:{end1.ToString("HH:mm:ss")}, job_started:{start3.ToString("HH:mm:ss")}, diff:{diffMinutes}\n");
                        }
                        if (diffMinutes > 60)
                        {
                            modelInfo.Comment = "delay 1hr+";
                            WriteDebugMessage($"[{now.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(07) {modelInfo.Comment}, pre_end:{end1.ToString("HH:mm:ss")}, job_started:{start3.ToString("HH:mm:ss")}, diff:{diffMinutes}\n");
                        }
                    }
                    else if (modelInfo.Comment == "delay 10+ mins" && now > end1)
                    {
                        if (diffMinutes > 30)
                        {
                            modelInfo.Comment = "delay 30+ mins";
                            WriteDebugMessage($"[{now.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(08) {modelInfo.Comment}, pre_end:{end1.ToString("HH:mm:ss")}, job_started:{start3.ToString("HH:mm:ss")}, diff:{diffMinutes}\n");
                        }
                        if (diffMinutes > 60)
                        {
                            modelInfo.Comment = "delay 1hr+";
                            WriteDebugMessage($"[{now.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(09) {modelInfo.Comment}, pre_end:{end1.ToString("HH:mm:ss")}, job_started:{start3.ToString("HH:mm:ss")}, diff:{diffMinutes}\n");
                        }
                    }
                    else if (modelInfo.Comment == "delay 30+ mins" && now > end1)
                    {
                        if (diffMinutes > 60)
                        {
                            modelInfo.Comment = "delay 1hr+";
                            WriteDebugMessage($"[{now.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(10) {modelInfo.Comment}, pre_end:{end1.ToString("HH:mm:ss")}, job_started:{start3.ToString("HH:mm:ss")}, diff:{diffMinutes}\n");
                        }
                    }

                    #region 監控檢查點 delay

                    //若有開啟檢查點監控機制才需要監控
                    if (checkPointLid == "ON")
                    {
                        var modelExeInfo = new CheckPointViewModelSearch
                        {
                            ModelName = modelInfo.Model_Name,
                            MemberName = modelInfo.Member_Name,
                            Account = modelInfo.Account,
                            CompleteRunType = modelInfo.Complete_Run_Type,
                            CronMode = modelInfo.Cron_Mode.ToString(),
                            TyphoonMode = (int)modelInfo.Typhoon_Mode,
                            Dtg = modelInfo.Dtg,
                            DtgHour = modelInfo.Dtg.Substring(modelInfo.Dtg.Length - 3),
                        };

                        var checkPoints = GetCheckPointInfos(modelExeInfo);
                        if (checkPoints.Any())
                        {
                            var modelStartTime = GetModelStartTime(modelExeInfo);
                            var excutingShell = GetExecutingShell(modelExeInfo);
                            var unRunCheckPoints = GetUnfinishCheckPoints(modelExeInfo, checkPoints, excutingShell);
                            UpdateDelayCheckPoint(modelExeInfo, unRunCheckPoints, modelStartTime);
                        }
                    }

                    #endregion 監控檢查點 delay
                }

                #endregion 工作結束延遲   (running)

                if (modelInfo.Manual > 0) modelInfo.Comment = "re-running";
                if (modelInfo.Status == "Cancelled") modelInfo.Comment = "Cancelled";

                #region consider next runtime  未啟動判斷 (not runnig)

                var nextTime = string.Empty;
                var cronStartTimes = cronInfos.Where(x => x.Model_Member_Nick == $"{modelInfo.Model_Name}{modelInfo.Member_Name}{modelInfo.Nickname}").Select(x => x.Start);

                //if (modelInfo.Status == "PAUSING" || modelInfo.Status == "Cancelled")
                //移除halt設計
                if (false)
                {
                    var lastCount = 0;

                    if (cronStartTimes.Any() == false) modelInfo.Status = $"next run at 12:00:00";
                    else
                    {
                        foreach (var cronStartTime in cronStartTimes)
                        {
                            var nowTime = DateTime.Now;
                            var lastTime = string.Empty;
                            var timeAdjust = modelInfo.Run_End > nowTime ? "*" : string.Empty;
                            var tmpCronStartTime = DateTime.Parse(now.ToString("yyyy/MM/dd") + " " + cronStartTime).AddSeconds(modelInfo.PreTime);
                            if (modelInfo.Run_End > nowTime) nowTime = nowTime.AddDays(1);
                            if (modelInfo.Run_End > tmpCronStartTime) tmpCronStartTime = tmpCronStartTime.AddDays(1);
                            if (modelInfo.Run_End < tmpCronStartTime && tmpCronStartTime < nowTime) //找出介於上次結束時間~現在時間的cron
                            {
                                lastCount++;
                                if (lastTime == string.Empty) //表示第一筆cron未啟動
                                {
                                    lastTime = cronStartTime;
                                    var diff = (nowTime.GetUnixTimestamp() - tmpCronStartTime.GetUnixTimestamp() - modelInfo.PreTime) / 60D;
                                    if (diff > 30)
                                    {
                                        modelInfo.Comment = "halt 30min+";
                                        WriteDebugMessage($"[{nowTime.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(11) {modelInfo.Comment}, last_run:{modelInfo.Run_End.ToString("HH/mm/ss")}, cron_start:{tmpCronStartTime.ToString("HH:mm:ss")}{timeAdjust}, diff:{diff}\n");
                                    }
                                    else if (diff > 5)
                                    {
                                        modelInfo.Comment = "halt 5min+";
                                        WriteDebugMessage($"[{nowTime.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(12) {modelInfo.Comment}, last_run:{modelInfo.Run_End.ToString("HH/mm/ss")}, cron_start:{tmpCronStartTime.ToString("HH:mm:ss")}{timeAdjust}, diff:{diff}\n");
                                    }
                                }
                                else //表示第二筆以上cron未啟動 (取最近)
                                {
                                    var lastTimeRun = DateTime.Parse(DateTime.Now.ToString("yyyy/MM/dd") + " " + lastTime);
                                    if (lastTimeRun < modelInfo.Run_End)
                                    {
                                        lastTimeRun = lastTimeRun.AddDays(1);
                                    }
                                    if (nowTime - lastTimeRun > nowTime - tmpCronStartTime)
                                    {
                                        lastTime = cronStartTime;
                                        var diff = (nowTime.GetUnixTimestamp() - tmpCronStartTime.GetUnixTimestamp()) / 60D;
                                        if (diff > 30)
                                        {
                                            modelInfo.Comment = "halt 30min+";
                                            WriteDebugMessage($"[{nowTime.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(13) {modelInfo.Comment}, last_run:{modelInfo.Run_End.ToString("HH/mm/ss")}, cron_start:{tmpCronStartTime.ToString("HH:mm:ss")}{timeAdjust}, diff:{diff}\n");
                                        }
                                        else if (diff > 5)
                                        {
                                            modelInfo.Comment = "halt 5min+";
                                            WriteDebugMessage($"[{nowTime.ToString("HH:mm:ss")}]{modelInfo.Model_Name}_{modelInfo.Member_Name}_{modelInfo.Nickname}(14) {modelInfo.Comment}, last_run:{modelInfo.Run_End.ToString("HH/mm/ss")}, cron_start:{tmpCronStartTime.ToString("HH:mm:ss")}{timeAdjust}, diff:{diff}\n");
                                        }
                                    }
                                }
                            }
                            if (nextTime == string.Empty)
                            {
                                nextTime = cronStartTime;
                            }
                            else
                            {
                                var nextTimeRun = DateTime.Parse(DateTime.Now.ToString("yyyy/MM/dd") + " " + nextTime);
                                while (tmpCronStartTime < nowTime)
                                {
                                    tmpCronStartTime = tmpCronStartTime.AddDays(1);
                                }
                                while (nextTimeRun < nowTime)
                                {
                                    nextTimeRun = nextTimeRun.AddDays(1);
                                }
                                if (tmpCronStartTime.GetUnixTimestamp() - nowTime.GetUnixTimestamp() < nextTimeRun.GetUnixTimestamp() - nowTime.GetUnixTimestamp())
                                {
                                    nextTime = cronStartTime;
                                }
                            }
                        }
                    }

                    if (lastCount > 0)
                    {
                        modelInfo.Comment += $"({lastCount})";
                    }
                    nextTime = nextTime == string.Empty ? "12:00:00" : nextTime;
                    modelInfo.Status = $"next run at {nextTime}";
                }

                #endregion consider next runtime  未啟動判斷 (not runnig)

                #region No matter what "$info->status" is, it is necessary to define $next_time.

                if (cronStartTimes.Any())
                {
                    foreach (var cronStartTime in cronStartTimes)
                    {
                        var nowTime = DateTime.Now;
                        var tmpCronStartTime = DateTime.Parse(now.ToString("yyyy/MM/dd") + " " + cronStartTime).AddSeconds(modelInfo.PreTime);
                        if (modelInfo.Run_End > nowTime) //若上次結束時間比"現在" 表跨天
                        {
                            nowTime = nowTime.AddDays(1);
                        }
                        if (modelInfo.Run_End > tmpCronStartTime) //若上次結束 時間比cron時間? 表跨天
                        {
                            tmpCronStartTime = tmpCronStartTime.AddDays(1);
                        }

                        if (nextTime == string.Empty)
                        {
                            nextTime = cronStartTime;
                        }
                        else
                        {
                            var nextTimeRun = DateTime.Parse(DateTime.Now.ToString("yyyy/MM/dd") + " " + nextTime);
                            while (tmpCronStartTime < nowTime)
                            {
                                tmpCronStartTime = tmpCronStartTime.AddDays(1);
                            }
                            while (nextTimeRun < nowTime)
                            {
                                nextTimeRun = nextTimeRun.AddDays(1);
                            }
                            if (tmpCronStartTime.GetUnixTimestamp() - nowTime.GetUnixTimestamp() < nextTimeRun.GetUnixTimestamp() - nowTime.GetUnixTimestamp())
                            {
                                nextTime = cronStartTime;
                            }
                        }
                    }
                }

                nextTime = nextTime == string.Empty ? "12:00:00" : nextTime;
                modelInfo.NextRun = $"next run at {nextTime}";

                #endregion No matter what "$info->status" is, it is necessary to define $next_time.
            }
        }

        private void WriteDebugMessage(string message)
        {
            _logFileService.WriteUiErrorLogFileAsync(message);
        }

        private void UpdateDelayCheckPoint(CheckPointViewModelSearch modelExeInfo, IEnumerable<CheckPointViewModel> unRunCheckPoints, DateTime modelStartTime)
        {
            var delayInfos = new List<CheckPointDelay>();
            foreach (var unRunCheckPoint in unRunCheckPoints)
            {
                var predictedEndTime = modelStartTime.AddSeconds(unRunCheckPoint.PredictEndSec);
                if (DateTime.Now <= predictedEndTime) continue;

                delayInfos.Add(new CheckPointDelay
                {
                    Cron_Mode = modelExeInfo.CronMode,
                    Typhoon_Mode = (Enums.TyphoonModeEnum)modelExeInfo.TyphoonMode,
                    Dtg = modelExeInfo.Dtg,
                    Check_Id = unRunCheckPoint.Check_Id,
                    Predicted_End_Time = predictedEndTime,
                    Model_Start_Time = modelStartTime,
                    Monitoring_Time = DateTime.Now,
                });
            }

            _dataBaseNcsUiService.InsertAsync("delayInfos", delayInfos);
        }

        private IEnumerable<CheckPointViewModel> GetUnfinishCheckPoints(CheckPointViewModelSearch modelExeInfo, IEnumerable<CheckPointViewModel> checkPoints, ExecutingShellViewModel excutingShell)
        {
            var sql = @"SELECT `batch_name`, `shell_name`, `avg_execution_time`
                        FROM `execution_time_result`
                        WHERE `model` = @ModelName
                        AND `member` = @MemberName
                        AND `account` = @Account
                        AND `run_type` = @CompleteRunType
                        AND `cron_mode` = @CronMode
                        AND `typhoon_mode` = @TyphoonMode
                        AND `round` = @DtgHour
                        AND `batch_name` = @BatchName
                        AND `shell_name` = @ShellName
                        ORDER BY `avg_execution_time` ASC";

            var param = new
            {
                ModelName = modelExeInfo.ModelName,
                MemberName = modelExeInfo.MemberName,
                Account = modelExeInfo.Account,
                CompleteRunType = modelExeInfo.CompleteRunType,
                CronMode = modelExeInfo.CronMode,
                TyphoonMode = modelExeInfo.TyphoonMode,
                DtgHour = modelExeInfo.DtgHour,
                BatchName = excutingShell.Batch_Name,
                ShellName = excutingShell.Shell_Name,
            };

            var result = _dataBaseNcsLogService.QueryAsync<UnfinishCheckPointViewModel>(sql, param).Result;
            return checkPoints.Where(x => result.Select(s => s.Shell_Name).Contains(x.Shell_Name)
                            && result.Select(s => s.Batch_Name).Contains(x.Batch_Name));
        }

        private ExecutingShellViewModel GetExecutingShell(CheckPointViewModelSearch modelExeInfo)
        {
            var sql = @"SELECT `batch_name`, `shell_name`
                        FROM `model_log`
                        WHERE `model` = @ModelName
                        AND `member` = @MemberName
                        AND `account` = @Account
                        AND `run_type` = @CompleteRunType
                        AND `cron_mode` = @CronMode
                        AND `typhoon_mode` = @TyphoonMode
                        AND `dtg` = @Dtg
                        AND `batch_name` != 'unknown'
                        AND `shell_name` != 'unknown'
                        AND `end_time` IS NOT NULL
                        AND `status` = 'finish'
                        AND `update_time` = (SELECT MAX(`update_time`) AS update_time
                                            FROM `model_log`
                                            WHERE `model` = @ModelName
                                            AND `member` =  @MemberName
                                            AND `account` = @Account
                                            AND `run_type` =  @CompleteRunType
                                            AND `cron_mode` = @CronMode
                                            AND `typhoon_mode` = @TyphoonMode
                                            AND `dtg` = @Dtg
                                            AND `batch_name` != 'unknown'
                                            AND `shell_name` != 'unknown'
                                            AND `end_time` IS NOT NULL
                                            AND `status` = 'finish')";

            return _dataBaseNcsLogService.QueryAsync<ExecutingShellViewModel>(sql, modelExeInfo).Result.FirstOrDefault();
        }

        private DateTime GetModelStartTime(CheckPointViewModelSearch modelExeInfo)
        {
            var sql = @"SELECT `start_time`
                        FROM `model_log`
                        WHERE `model` = @ModelName
                        AND `member` = @MemberName
                        AND `account` = @Account
                        AND `run_type` = @CompleteRunType
                        AND `cron_mode` = @CronMode
                        AND `typhoon_mode` = @TyphoonMode
                        AND `dtg` = @Dtg
                        AND `batch_name` = 'unknown'
                        AND `shell_name` = 'unknown'";
            return _dataBaseNcsLogService.QueryAsync<DateTime>(sql, modelExeInfo).Result.FirstOrDefault();
        }

        private IEnumerable<CheckPointViewModel> GetCheckPointInfos(CheckPointViewModelSearch param)
        {
            var sql = @"SELECT `model`.`model_name`, `member`.`member_name`, `member`.`nickname`, `member`.`account`
                            , `batch`.`batch_name`, `batch`.`batch_type`, `batch`.`batch_dtg`
                            , `check_point`.`check_id`, `check_point`.`shell_name`, `check_point`.`tolerance_time`
                        FROM `check_point`
                        JOIN `member` ON `member`.`member_id` = `check_point`.`member_id`
                        JOIN `batch` ON `batch`.`batch_id` = `check_point`.`batch_id`
                        JOIN `model` ON `model`.`model_id` = `member`.`model_id`
                        WHERE `model`.`model_name` = @ModelName
                        AND `member`.`member_name` = @MemberName
                        AND `member`.`account` = @Account
                        AND (`batch`.`batch_dtg` = '' OR `batch`.`batch_dtg` = @DtgHour
                        AND (`batch`.`batch_type` = '' OR `batch`.`batch_type` = @CompleteRunType
                        ORDER BY `check_point`.`batch_id` ASC, `check_point`.`check_id` ASC";

            var result = _dataBaseNcsUiService.QueryAsync<CheckPointViewModel>(sql, param).Result;
            foreach (var checkPoint in result)
            {
                var avgExecutionTime = GetShellAvgTime(param, checkPoint);
                if (avgExecutionTime.HasValue)
                {
                    checkPoint.AvgExecutionTime = avgExecutionTime.Value;
                }
            }

            return result;
        }

        private int? GetShellAvgTime(CheckPointViewModelSearch modelParam, CheckPointViewModel checkPoint)
        {
            var sql = @"SELECT `avg_execution_time`
                        FROM `execution_time_result`
                        WHERE `model` = @ModelName
                        AND `member` = @MemberName
                        AND `account` = @Account
                        AND `typhoon_mode` = @TyphoonMode
                        AND `run_type` = @RunType
                        AND `cron_mode` = @CronMode
                        AND `round` = @Round
                        AND `batch_name` = @BatchName
                        AND `shell_name` = @ShellName";

            var param = new
            {
                ModelName = checkPoint.Model_Name,
                MemberName = checkPoint.Member_Name,
                Account = checkPoint.Account,
                TyphoonMode = modelParam.TyphoonMode,
                RunType = modelParam.CompleteRunType,
                CronMode = modelParam.CronMode,
                Round = modelParam.DtgHour,
                BatchName = checkPoint.Batch_Name,
                ShellName = checkPoint.Shell_Name,
            };

            return _dataBaseNcsLogService.QueryAsync<int>(sql, param).Result.FirstOrDefault();
        }

        private IEnumerable<BatchInfoViewModel> GetShowBatchInfos()
        {
            var sql = @"SELECT CONCAT(`model`.`model_name`,`member`.`member_name`,`member`.`nickname`) AS model_member_nick,
                            `batch`.`batch_name` AS `batch`,
                            `batch`.`batch_position` AS `position`,
                            `batch`.`batch_type` AS `type`,
                            `batch`.`batch_dtg` AS `dtg`,
                            `batch`.`batch_time` AS `time`
                        FROM
                            ((`model` JOIN `member`) JOIN `batch`)
                        WHERE
                            ((`model`.`model_id` = `member`.`model_id`)
                            AND (`member`.`member_id` = `batch`.`member_id`))
                        ORDER BY
                            model_member_nick,
                            `batch`.`batch_position`";

            return _dataBaseNcsUiService.QueryAsync<BatchInfoViewModel>(sql).Result;
        }

        private IEnumerable<CronInfoViewModel> GetShowCronInfos()
        {
            var sql = @"SELECT
                            CONCAT(a.model_name, b.member_name, b.nickname) AS model_member_nick,
                            c.start_time as `start`
                        FROM
                            model as a,
                            member as b,
                            cron_view as c
                        WHERE
                            a.model_id=b.model_id
                            AND b.member_id=c.member_id";

            return _dataBaseNcsUiService.QueryAsync<CronInfoViewModel>(sql).Result;
        }

        private IEnumerable<ModelConfigViewModel> GetShowModelConfigs()
        {
            var sql = @"SELECT `member`.*
                        , `model`.`model_name`
                        , `model`.`model_position`
                        ,`monitoring_info`.`lid`
                        ,`monitoring_info`.`dtg`
                        ,`monitoring_info`.`run`
                        ,`monitoring_info`.`complete_run_type`
                        ,`monitoring_info`.`run_type`
                        ,`monitoring_info`.`cron_mode`
                        ,`monitoring_info`.`typhoon_mode`
                        ,`monitoring_info`.`manual`
                        ,`monitoring_info`.`start_flag`
                        ,`monitoring_info`.`stage_flag`
                        ,`monitoring_info`.`status`
                        ,`monitoring_info`.`sms_name`
                        ,CONCAT(DATE_FORMAT(curdate(),'%Y/%m/%d'), ' ', `monitoring_info`.`sms_time`) AS `sms_time`
                        ,CONCAT(DATE_FORMAT(curdate(),'%Y/%m/%d'), ' ', `monitoring_info`.`start_time`) AS `start_time`
                        ,CONCAT(DATE_FORMAT(curdate(),'%Y/%m/%d'), ' ', `monitoring_info`.`end_time`) AS `end_time`
                        ,CONCAT(DATE_FORMAT(curdate(),'%Y/%m/%d'), ' ', `monitoring_info`.`pre_start`) AS `pre_start`
                        ,CONCAT(DATE_FORMAT(curdate(),'%Y/%m/%d'), ' ', `monitoring_info`.`pre_end`) AS `pre_end`
                        ,CONCAT(DATE_FORMAT(curdate(),'%Y/%m/%d'), ' ', `monitoring_info`.`run_end`) AS `run_end`
                        ,`monitoring_info`.`shell_name`
                        ,CONCAT(DATE_FORMAT(curdate(),'%Y/%m/%d'), ' ', `monitoring_info`.`shell_time`) AS `shell_time`
                        ,`monitoring_info`.`error_message`
                        FROM `member`
                        INNER JOIN `model` ON `model`.`model_id` = `member`.`model_id`
                        LEFT JOIN `monitoring_info` ON `monitoring_info`.`model` = `model`.`model_name`
                                                   AND  `monitoring_info`.`member` = `member`.`member_name`
                                                   AND `monitoring_info`.`nickname` = `member`.`nickname`
                                                   AND `monitoring_info`.`account` = `member`.`account`
                        ORDER BY `model`.`model_position` ASC, `member`.`member_position` ASC";

            return _dataBaseNcsUiService.QueryAsync<ModelConfigViewModel>(sql).Result;
        }

        private int GetPreTime(string modelName, string memberName, string nickname)
        {
            var defaultPreTime = 300;
            var sql = $@"SELECT M.`model_name`, M.`member_name`, M.`nickname`, M.`typhoon_pre_time`, M.`normal_pre_time`, C.`cron_group`, C.`group_validation`
                         FROM `model_member_view` M
                         LEFT JOIN crontab C ON (M.`member_id` = C.`member_id`)
                         WHERE C.`group_validation` = '1'
                         AND M.`model_name` = @ModelName
                         AND M.`member_name` = @MemberName
                         AND M.`nickname` = @Nickname
                         GROUP BY M.member_id, model_name, member_name, nickname, typhoon_pre_time, normal_pre_time, cron_group, group_validation";
            var result = _dataBaseNcsUiService.QueryAsync<PreTimeByModelMemberViewModel>(sql, new { ModelName = modelName, MemberName = memberName, Nickname = nickname }).Result.FirstOrDefault();

            if (result == null) return defaultPreTime;
            if (string.IsNullOrWhiteSpace(result.Cron_Group)) return defaultPreTime;
            if (result.Cron_Group.ToLower() == "normal") return result.Normal_Pre_Time;
            if (result.Cron_Group.ToLower() == "typhoon") return result.Typhoon_Pre_Time;

            return defaultPreTime;
        }

        private async Task<UploadFile> GetUploadFileItemAsync(int fileId)
        {
            var data = await _dataBaseNcsUiService.GetAllAsync<UploadFile>("upload_file", new { file_id = fileId });
            return data.FirstOrDefault();
        }

        private async void EditDump(string fileName)
        {
            var hpcSql = _dataBaseNcsUiService.DataBaseName;
            var account = _dataBaseNcsUiService.DataBaseUid;
            var password = _dataBaseNcsUiService.DataBasePwd;
            var baseDir = $"/{_SystemName}/{_HpcCtl}/web";

            var options = $"--ignore-table={hpcSql}.history_batch";
            options += $" --ignore-table={hpcSql}.history_batch_model_view";
            options += $" --ignore-table={hpcSql}.history_batch_stage_view";
            options += $" --ignore-table={hpcSql}.archive_view";
            options += $" --ignore-table={hpcSql}.batch_view";
            options += $" --ignore-table={hpcSql}.cron_view";
            options += $" --ignore-table={hpcSql}.model_member_view";
            options += $" --ignore-table={hpcSql}.model_view";
            options += $" --ignore-table={hpcSql}.ouput_view";
            options += $" --ignore-table={hpcSql}.user_view";

            //var sqldump = $"sudo -u {_RshAccount} mysqldump -u{account} -p{password} {hpcSql} {options} > {fileName}";
            var sqldump = $"sudo -u {_RshAccount} mysqldump -u{account} -p{password} {hpcSql} > {fileName}";
            var dump = await RunCommandAsync(sqldump);
            /* var dumparr = Regex.Split(dump, "/\n /");
            foreach (var i in dumparr)
            {
                var printStr = string.Empty;
                if (Regex.IsMatch(i, "/^INSERT INTO .+\\(.+$/", RegexOptions.IgnoreCase))
                {
                    var SQLDATA = Regex.Replace(i, "/\\)\\,/", ");\n");
                    var SQLarr = Regex.Split(SQLDATA, "/\n/");
                    foreach (var j in SQLarr)
                    {
                        var prefix = string.Empty;

                        if (Regex.IsMatch(j, "/INSERT INTO/", RegexOptions.IgnoreCase))
                        {
                            var tmparr = Regex.Split(j, "/\\(/");
                            prefix = tmparr[0];
                            printStr = $"{j}\n";
                        }
                        else
                        {
                            printStr = $"{prefix}{j}\n";
                        }
                        _logFileService.WriteDataIntoLogFileAsync(baseDir, fileName, printStr);
                    }
                }
                else
                {
                    printStr = $"{i}\n";
                    _logFileService.WriteDataIntoLogFileAsync(baseDir, fileName, printStr);
                }
            }*/
        }

        #endregion Private Methods
    }
}