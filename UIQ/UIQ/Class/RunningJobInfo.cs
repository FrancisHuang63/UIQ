using UIQ.Enums;

namespace UIQ
{
    public class RunningJobInfoOption
    {
        public IEnumerable<RunningJobInfo> RunningJobInfos { get; set; }

        public RunningJobInfo GetRunningJobInfo(string baseUrl) => RunningJobInfos?.FirstOrDefault(x => x.BaseUrl == baseUrl);
    }

    public class RunningJobInfo
    {
        public string BaseUrl { get; set; }

        public IndexSideEnum IndexSide { get; set; }

        public IEnumerable<RunningJobInfoItem> Items { get; set; }
    }

    public class RunningJobInfoItem
    {
        public string Key { get; set; }

        public IEnumerable<RunningJobInfoData> Datas { get; set; }
    }

    public class RunningJobInfoData
    {
        public string LoginIp { get; set; }

        public string DataMvIp { get; set; }

        public string Name { get; set; }

        public string CronIp { get; set; }

        public string Prefix { get; set; }
    }
}