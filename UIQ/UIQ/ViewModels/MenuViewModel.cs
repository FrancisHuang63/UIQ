namespace UIQ.ViewModels
{
    public class MenuViewModel
    {
        public string Title { get; set; }

        public string LinkUrl { get; set; }

        public int Sort { get; set; }

        public List<MenuViewModel> ChildItems { get; set; }

        public MenuViewModel(string title, string linkUrl, int sort)
        {
            Title = title;
            LinkUrl = linkUrl;
            Sort = sort;
        }

        public MenuViewModel(string title, int sort, IEnumerable<MenuViewModel> childItem)
        {
            Title = title;
            Sort = sort;
            ChildItems = childItem.ToList();
        }

        public MenuViewModel()
        {

        }
    }
}