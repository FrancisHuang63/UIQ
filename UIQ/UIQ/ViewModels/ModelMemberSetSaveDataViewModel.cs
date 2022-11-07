using UIQ.Models;

namespace UIQ.ViewModels
{
    public class ModelMemberSetSaveDataViewModel
    {
        public bool IsNew { get; set; }

        public bool IsNewModelName { get; set; }

        public string New_Model_Name { get; set; }

        public int New_Model_Position { get; set; }

        public Model Model { get; set; }

        public Member Member { get; set; }

        public IEnumerable<CronTab> CronTabs { get; set; }

        public IEnumerable<Batch> Batchs { get; set; }

        public IEnumerable<Archive> Archives { get; set; }

        public IEnumerable<Output> Outputs { get; set; }

        public IEnumerable<CheckPointSaveViewModel> CheckPoints { get; set; }
    }
}
