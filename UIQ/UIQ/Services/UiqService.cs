using System.ComponentModel.Design;
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
        private readonly ISshCommandService _sshCommandService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _HpcCtl { get; set; }
        private string _RshAccount { get; set; }

        public UiqService(IHttpContextAccessor httpContextAccessor, IEnumerable<IDataBaseService> dataBaseServices, ISshCommandService sshCommandService, IConfiguration configuration)
        {
            _dataBaseNcsUiService = dataBaseServices.Single(x => x.DataBase == Enums.DataBaseEnum.NcsUi);
            _dataBaseNcsLogService = dataBaseServices.Single(x => x.DataBase == Enums.DataBaseEnum.NcsLog);
            _sshCommandService = sshCommandService;
            _httpContextAccessor = httpContextAccessor;
            _HpcCtl = configuration.GetValue<string>("HpcCTL");
            _RshAccount = configuration.GetValue<string>("RshAccount");
        }

        public IEnumerable<HomeTableViewModel> GetHomeTableDatas()
        {
            var command = string.Empty;
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
                    AlertFlag = (modelConfig.Status.ToUpper() == "FAIL" && modelConfig.Lid != Enums.LidEnum.Zero && modelConfig.Comment != "Cancelled"),
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

        public async Task<string> GetExecuteNwpRunningNodesCommandHtmlAsync(string selNode)
        {
            if (!string.IsNullOrWhiteSpace(selNode)) return string.Empty;

            var resultHtml = string.Empty;
            var nodes = selNode.Split(',');
            foreach (var node in nodes)
            {
                var command = $"rsh -l ${_RshAccount} {node} /ncs/${_HpcCtl}/web/shell/ps.ksh";

                resultHtml += "<pre>";
                resultHtml += $"<h3>{node}</h3>";
                resultHtml += "---------------------------------------------------------------------------------------\n";
                resultHtml += await RunCommandAsync(command);
                resultHtml += "---------------------------------------------------------------------------------------</pre>\n\n";
            }

            return resultHtml;
        }

        public IEnumerable<ModelLogFileViewModel> GetModelLogFileViewModels()
        {
            var sql = @"SELECT `model`.`model_name`,
                        `member`.`model_id`,
                        `member`.`member_name`,
                        `member`.`nickname`,
                        `member`.`account`,
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
            var sql = @"SELECT `member`.`account`, `member`.`member_path`
                        FROM `member`
                        LEFT JOIN `model` ON `member`.`model_id` = `model`.`model_id`
                        WHERE `model`.`model_name` = @modelName
                        AND `member`.`member_name` = @memberName
                        AND `member`.`nickname` = @nickname";

            var datas = await _dataBaseNcsUiService.QueryAsync<FullPathViewModel>(sql, new { modelName = modelName, memberName = memberName, nickname = nickname });

            var result = datas.FirstOrDefault();
            if (result == null) return null;

            return $"/ncs/{result.Account}{result.Member_Path}/{modelName}/{memberName}";
        }

        public async Task<IEnumerable<Model>> GetModelItemsAsync()
        {
            var datas = await _dataBaseNcsUiService.GetAllAsync<Model>(nameof(Model));
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
                            (SELECT SUM(bt.`batch_time`) FROM `db_ncsui`.`batch` bt
	                         WHERE bt.`member_id` = mb.`member_id`
                             AND (bt.`batch_type` = '' OR bt.`batch_type` = SUBSTRING_INDEX(etr.`run_type`, '_', 1))
                             AND (bt.`batch_dtg` = '' OR bt.`batch_dtg` = etr.`round`)
                             GROUP BY(bt.`member_id`)
	                        ) AS `batch_time`
                        FROM `db_ncs_log`.`execution_time_result` etr
                        JOIN `db_ncsui`.`member` mb ON mb.`member_name` = etr.`member` AND mb.`account` = etr.`account`
                        JOIN `db_ncsui`.`model` md ON md.`model_name` = etr.`model` AND md.`model_id` = mb.`model_id`
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
                                        FROM `db_ncsui`.`member` mb
                                        JOIN `db_ncsui`.`model` md ON md.`model_id` = mb.`model_id`
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
                                        FROM `db_ncsui`.`member` mb
                                        JOIN `db_ncsui`.`model` md ON md.`model_id` = mb.`model_id`
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
                            `model`.`model_id`, `model`.`model_name`, `member`.`member_name`, `member`.`nickname`, `member`.`account`
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
            if (data.Command_Id.HasValue)
                return await _dataBaseNcsUiService.UpdateAsync("command", data, new { Command_Id = data.Command_Id }) > 0;

            return await _dataBaseNcsUiService.InsertAsync("command", data) > 0;
        }

        public async Task<bool> DeleteCommandAsync(int commandId)
        {
            return await _dataBaseNcsUiService.DeleteAsync("command", new { Command_Id = commandId }) > 0;
        }

        public async Task<Command> GetCommandItemAsync(int commandId)
        {
            var sql = @"SELECT * FROM `command` WHERE `command_id` = @CommandId";
            return (await _dataBaseNcsUiService.QueryAsync<Command>(sql, new { CommandId = commandId })).FirstOrDefault();
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

        public async Task<Member> GetMemberItemAsync(int memberId)
        {
            var result = await _dataBaseNcsUiService.GetAllAsync<Member>("member", new { member_id = memberId });
            return result.FirstOrDefault();
        }

        public async Task<Member> GetMemberItemAsync(string modelName, string memberName, string nickname)
        {
            var sql = @"SELECT `member`.*
                        FROM `member` mb
                        LEFT JOIN `model` md ON md.`model_id` = mb.`model_id`
                        WHERE md.`model_name` = @model_name
                        AND mb.`member_name` = @member_name
                        AND mb.`nickname` = @nickname";
            var result = await _dataBaseNcsUiService.GetAllAsync<Member>("member", new { model_name = modelName, member_name = memberName, nickname = nickname });
            return result.FirstOrDefault();
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

        public async Task<bool> SaveModelMemberSetData(ModelMemberSetSaveDataViewModel data)
        {
            var result = 0;
            var memberId = data.Member.Member_Id;

            //Model
            if (data.IsNewModelName)
            {
                data.Model = new Model { Model_Name = data.New_Model_Name, Model_Position = data.New_Model_Position, };
                data.Member.Model_Id = (int)await _dataBaseNcsUiService.InsertAndReturnAutoGenerateIdAsync("model", data.Model);
            }

            //Member
            var isMemberExist = await _dataBaseNcsUiService.IsExistAsync("member", new { Member_Id = data.Member.Member_Id });
            if (isMemberExist) result += await _dataBaseNcsUiService.UpdateAsync("member", data.Member, new { Member_Id = data.Member.Member_Id });
            else memberId = (int)await _dataBaseNcsUiService.InsertAndReturnAutoGenerateIdAsync("member", data.Member);

            //CronTab
            foreach (var cronTab in data.CronTabs)
            {
                cronTab.Member_Id = memberId;
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

            //Batch
            foreach (var batch in data.Batchs)
            {
                batch.Member_Id = memberId;
                var paramter = new { Batch_Id = batch.Batch_Id };
                var isExist = await _dataBaseNcsUiService.IsExistAsync("batch", paramter);
                if (isExist) result += await _dataBaseNcsUiService.UpdateAsync("batch", batch, paramter);
                else result += await _dataBaseNcsUiService.InsertAsync("batch", batch);
            }

            //Archive
            foreach (var archive in data.Archives)
            {
                archive.Member_Id = memberId;
                var paramter = new { Archive_Id = archive.Archive_Id };
                var isExist = await _dataBaseNcsUiService.IsExistAsync("archive", paramter);
                if (isExist) result += await _dataBaseNcsUiService.UpdateAsync("archive", archive, paramter);
                else result += await _dataBaseNcsUiService.InsertAsync("archive", archive);
            }

            //Output
            foreach (var output in data.Outputs)
            {
                output.Member_Id = memberId;
                var paramter = new { Output_Id = output.Output_Id };
                var isExist = await _dataBaseNcsUiService.IsExistAsync("output", paramter);
                if (isExist) result += await _dataBaseNcsUiService.UpdateAsync("output", output, paramter);
                else result += await _dataBaseNcsUiService.InsertAsync("output", output);
            }

            return result > 0;
        }

        public async Task<IEnumerable<UploadFile>> GetUploadFileItemsAsync()
        {
            return await _dataBaseNcsUiService.GetAllAsync<UploadFile>("upload_file");
        }

        public async Task<IEnumerable<Role>> GetRoleItemsAsync()
        {
            return await _dataBaseNcsUiService.GetAllAsync<Role>("role");
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

        #region Private Methods

        private void Parse(IEnumerable<ModelConfigViewModel> modelInfos, IEnumerable<CronInfoViewModel> cronInfos, string checkPointLid)
        {
            foreach (var modelInfo in modelInfos)
            {
                if ($"{modelInfo.Model_Name}_{modelInfo.Member_Name}({modelInfo.Nickname})" == "GFS_MNH(T511)")
                {
                }
                var maxPerRunTime = modelInfo.Member_Dtg_Value * 60 * 60;
                if (modelInfo.Status == "Cancelled" || modelInfo.Status.ToUpper() == "FAIL")
                    WriteDebugMessage($"[{DateTime.Now.ToString("HH:mm:ss")}]${modelInfo.Model_Name}_${modelInfo.Member_Name}_${modelInfo.Nickname}(C) modelInfo->status, run_end:{modelInfo.Run_End}\n");

                modelInfo.PreTime = GetPreTime(modelInfo.Model_Name, modelInfo.Member_Name, modelInfo.Nickname);

                #region consider delay 工作 啟始延遲判斷   (running)

                var preStartTime = new DateTime(modelInfo.Pre_Start.Ticks);
                var now = DateTime.Now;
                var workStartTime = new DateTime(modelInfo.Sms_Time.Ticks);

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
                    WriteDebugMessage("[now]${model}_${member}_${nickname}(01) modelInfo.Comment, pre_start:${start1}, job_started:${start3}, diff:${diff}\n");
                }
                if (diffMinutes > 30)
                {
                    modelInfo.Comment = "delay 30+ mins";
                    WriteDebugMessage("[now]${model}_${member}_${nickname}(02) modelInfo.Comment, pre_start:${start1}, job_started:${start3}, diff:${diff}\n");
                }
                if (diffMinutes > 60)
                {
                    modelInfo.Comment = "delay 1hr+";
                    WriteDebugMessage("[now]${model}_${member}_${nickname}(03) modelInfo.Comment, pre_start:${start1}, job_started:${start3}, diff:${diff}\n");
                }
                //if (modelInfo.SmsTime == "")
                //{
                //    modelInfo.Comment = "unknown";
                //    WriteDebugMessage("[now]${model}_${member}_${nickname}(04) modelInfo.Comment, pre_start:${start1}, job_started:${start3}, diff:${diff}\n");
                //}

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
                            WriteDebugMessage("[now]${model}_${member}_${nickname}(05) modelInfo.Comment, pre_end:${end1}, job_started:${start3}, diff:${diff}\n");
                        }
                        if (diffMinutes > 30)
                        {
                            modelInfo.Comment = "delay 30+ mins";
                            WriteDebugMessage("[now]${model}_${member}_${nickname}(06) modelInfo.Comment, pre_end:${end1}, job_started:${start3}, diff:${diff}\n");
                        }
                        if (diffMinutes > 60)
                        {
                            modelInfo.Comment = "delay 1hr+";
                            WriteDebugMessage("[now]${model}_${member}_${nickname}(07) modelInfo.Comment, pre_end:${end1}, job_started:${start3}, diff:${diff}\n");
                        }
                    }
                    else if (modelInfo.Comment == "delay 10+ mins" && now > end1)
                    {
                        if (diffMinutes > 30)
                        {
                            modelInfo.Comment = "delay 30+ mins";
                            WriteDebugMessage("[now]${model}_${member}_${nickname}(08) modelInfo.Comment, pre_end:${end1}, job_started:${start3}, diff:${diff}\n");
                        }
                        if (diffMinutes > 60)
                        {
                            modelInfo.Comment = "delay 1hr+";
                            WriteDebugMessage("[now]${model}_${member}_${nickname}(09) modelInfo.Comment, pre_end:${end1}, job_started:${start3}, diff:${diff}\n");
                        }
                    }
                    else if (modelInfo.Comment == "delay 30+ mins" && now > end1)
                    {
                        if (diffMinutes > 60)
                        {
                            modelInfo.Comment = "delay 1hr+";
                            WriteDebugMessage("[now]${model}_${member}_${nickname}(10) modelInfo.Comment, pre_end:${end1}, job_started:${start3}, diff:${diff}\n");
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
                if (modelInfo.Status == "PAUSING" || modelInfo.Status == "Cancelled")
                {
                    var lastCount = 0;

                    if (cronStartTimes.Any() == false) modelInfo.Status = $"next run at 12:00:00";
                    else
                    {
                        foreach (var cronStartTime in cronStartTimes)
                        {
                            var nowTime = DateTime.Now;
                            var lastTime = string.Empty;
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
                                        WriteDebugMessage("[nowTime]${model}_${member}_${nickname}(11) modelInfo->comment, last_run:${last_run}, cron_start:${cron_start}${time_adjust}, diff:${diff}\n");
                                    }
                                    else if (diff > 5)
                                    {
                                        modelInfo.Comment = "halt 5min+";
                                        WriteDebugMessage("[nowTime]${model}_${member}_${nickname}(12) modelInfo->comment, last_run:${last_run}, cron_start:${cron_start}${time_adjust}, diff:${diff}\n");
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
                                            WriteDebugMessage("[nowTime]${model}_${member}_${nickname}(13) modelInfo->comment, last_run:${last_run}, cron_start:${cron_start}${time_adjust}, diff:${diff}\n");
                                        }
                                        else if (diff > 5)
                                        {
                                            modelInfo.Comment = "halt 5min+";
                                            WriteDebugMessage("[nowTime]${model}_${member}_${nickname}(14) modelInfo->comment, last_run:${last_run}, cron_start:${cron_start}${time_adjust}, diff:${diff}\n");
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
                        if (modelInfo.Run_End > nowTime) //若上次結束時間比"現在"? 表跨天
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
                        FROM `member`, `model`, `monitoring_info`
                        WHERE `member`.`model_id` = `model`.`model_id`
                        AND `monitoring_info`.`model` = `model`.`model_name`
                        AND `monitoring_info`.`member` = `member`.`member_name`
                        AND `monitoring_info`.`nickname` = `member`.`nickname`
                        ORDER BY `model_position` ASC, `member_position` ASC";

            return _dataBaseNcsUiService.QueryAsync<ModelConfigViewModel>(sql).Result;
        }

        private void WriteDebugMessage(string message)
        {
            //TODO
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

        #endregion Private Methods
    }
}