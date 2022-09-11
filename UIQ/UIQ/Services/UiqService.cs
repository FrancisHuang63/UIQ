﻿using UIQ.Models;
using UIQ.Services.Interfaces;
using UIQ.ViewModels;

namespace UIQ.Services
{
    public class UiqService : IUiqService
    {
        private IDataBaseService _dataBaseNcsUiService;
        private IDataBaseService _dataBaseNcsLogService;

        public UiqService(IEnumerable<IDataBaseService> dataBaseServices)
        {
            _dataBaseNcsUiService = dataBaseServices.Single(x => x.DataBase == Enums.DataBaseEnum.NcsUi);
            _dataBaseNcsLogService = dataBaseServices.Single(x => x.DataBase == Enums.DataBaseEnum.NcsLog);
        }

        public IEnumerable<HomeTableViewModel> GetHomeTableDatas()
        {
            var resultDatas = new List<HomeTableViewModel>();
            var batchInfos = GetShowBatchInfos();
            var cronInfos = GetShowCronInfos();
            var modelConfigs = GetShowModelConfigs();

            return resultDatas;
        }

        public IEnumerable<Member> GetMembers()
        {
            return _dataBaseNcsUiService.GetAllAsync<Member>("member").Result.OrderBy(x => x.MemberPosition);
        }

        public IEnumerable<Model> GetModels()
        {
            return _dataBaseNcsUiService.GetAllAsync<Model>("model").Result.OrderBy(x => x.ModelPosition);
        }

        public IEnumerable<ModelConfigViewModel> Parse(IEnumerable<ModelConfigViewModel> modelInfos, IEnumerable<CronInfoViewModel> cronInfos, string checkPointLid)
        {
            foreach (var modelInfo in modelInfos)
            {
                var maxPerRunTime = modelInfo.MemberDtgValue * 60 * 60;
                if (modelInfo.Status == "Cancelled" || modelInfo.Status.ToUpper() == "FAIL")
                    WriteDebugMessage($"[{DateTime.Now.ToString("HH:mm:ss")}]${modelInfo.ModelName}_${modelInfo.MemberName}_${modelInfo.Nickname}(C) modelInfo->status, run_end:{modelInfo.RunEnd}\n");

                modelInfo.PreTime = GetPreTime(modelInfo.ModelName, modelInfo.MemberName, modelInfo.Nickname);

                #region consider delay 工作 啟始延遲判斷   (running)

                var preStartTime = new DateTime(modelInfo.PreStart.Ticks);
                var now = DateTime.Now;
                var workStartTime = new DateTime(modelInfo.SmsTime.Ticks);

                //若工作起始時間 > 現在時間 表跨天
                if (workStartTime > now) workStartTime = workStartTime.AddDays(-1);

                //若預測起始時間 > 現在時間 表跨天 (時間差異應6小時以上)
                if (preStartTime > now && (preStartTime.Ticks - now.Ticks) > maxPerRunTime) preStartTime = preStartTime.AddDays(-1);

                //若預測起始時間 < 現在時間 表跨天 (時間差異應6小時以上)
                if (preStartTime < now && (now.Ticks - preStartTime.Ticks) > maxPerRunTime) preStartTime = preStartTime.AddDays(1);

                var diffMinutes = (workStartTime.Ticks - preStartTime.Ticks) / 60;
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

                var end1 = new DateTime(modelInfo.PreEnd.Ticks);  //預測結束時間
                if (modelInfo.Status == "RUNNING")
                {
                    var start3 = new DateTime(modelInfo.StartTime.Ticks);    //工作起始時間
                    if (now < start3)
                    {
                        start3 = start3.AddDays(-2);
                        end1 = (start3 < end1 && (end1.Ticks - start3.Ticks) > maxPerRunTime)
                            ? end1.AddDays(-2)
                            : end1;
                    }
                    else
                    {
                        end1 = start3 > end1 ? end1.AddDays(2) : end1;
                    }

                    diffMinutes = (now.Ticks - end1.Ticks) / 60;
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
                            ModelName = modelInfo.ModelName,
                            MemberName = modelInfo.MemberName,
                            Account = modelInfo.Account,
                            CompleteRunType = modelInfo.CompleteRunType,
                            CronMode = modelInfo.CronMode.ToString(),
                            TyphoonMode = (int)modelInfo.TyphoonMode,
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
                var cronStartTimes = cronInfos.Where(x => x.ModelMemberNick == $"{modelInfo.ModelName}{modelInfo.MemberName}{modelInfo.Nickname}").Select(x => x.Start);
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
                            if (modelInfo.RunEnd > nowTime) nowTime.AddDays(1);
                            if (modelInfo.RunEnd > tmpCronStartTime) tmpCronStartTime.AddDays(1);
                            if (modelInfo.RunEnd < tmpCronStartTime && tmpCronStartTime < nowTime) //找出介於上次結束時間~現在時間的cron
                            {
                                lastCount++;
                                if (lastTime == string.Empty) //表示第一筆cron未啟動
                                {
                                    lastTime = cronStartTime;
                                    var diff = (nowTime.Ticks - tmpCronStartTime.Ticks - modelInfo.PreTime) / 60;
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
                                    if (lastTimeRun < modelInfo.RunEnd)
                                    {
                                        lastTimeRun = lastTimeRun.AddDays(1);
                                    }
                                    if (nowTime - lastTimeRun > nowTime - tmpCronStartTime)
                                    {
                                        lastTime = cronStartTime;
                                        var diff = (nowTime.Ticks - tmpCronStartTime.Ticks) / 60;
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
                                if (tmpCronStartTime.Ticks - nowTime.Ticks < nextTimeRun.Ticks - nowTime.Ticks)
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
                        if (modelInfo.RunEnd > nowTime) //若上次結束時間比"現在"? 表跨天
                        {
                            nowTime = nowTime.AddDays(1);
                        }
                        if (modelInfo.RunEnd > tmpCronStartTime) //若上次結束 時間比cron時間? 表跨天
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
                            if (tmpCronStartTime.Ticks - nowTime.Ticks < nextTimeRun.Ticks - nowTime.Ticks)
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

            return modelInfos;
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
                    CronMode = modelExeInfo.CronMode,
                    TyphoonMode = (Enums.TyphoonModeEnum)modelExeInfo.TyphoonMode,
                    Dtg = modelExeInfo.Dtg,
                    CheckId = unRunCheckPoint.CheckId,
                    PredictedEndTime = predictedEndTime,
                    ModelStartTime = modelStartTime,
                    MonitoringTime = DateTime.Now,
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
                BatchName = excutingShell.BatchName,
                ShellName = excutingShell.ShellName,
            };

            var result = _dataBaseNcsLogService.QueryAsync<UnfinishCheckPointViewModel>(sql, param).Result;
            return checkPoints.Where(x => result.Select(s => s.ShellName).Contains(x.ShellName)
                            && result.Select(s => s.BatchName).Contains(x.BatchName));
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
                ModelName = checkPoint.ModelName,
                MemberName = checkPoint.MemberName,
                Account = checkPoint.Account,
                TyphoonMode = modelParam.TyphoonMode,
                RunType = modelParam.CompleteRunType,
                CronMode = modelParam.CronMode,
                Round = modelParam.DtgHour,
                BatchName = checkPoint.BatchName,
                ShellName = checkPoint.ShellName,
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
                            ,`monitoring_info`.`sms_time`
                            ,`monitoring_info`.`start_time`
                            ,`monitoring_info`.`end_time`
                            ,`monitoring_info`.`pre_start`
                            ,`monitoring_info`.`pre_end`
                            ,`monitoring_info`.`run_end`
                            ,`monitoring_info`.`shell_name`
                            ,`monitoring_info`.`shell_time`
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
            if (string.IsNullOrWhiteSpace(result.CronGroup)) return defaultPreTime;
            if (result.CronGroup.ToLower() == "normal") return result.NormalPreTime;
            if (result.CronGroup.ToLower() == "typhoon") return result.TyphoonPreTime;

            return defaultPreTime;
        }
    }
}