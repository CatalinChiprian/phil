namespace PHIL_GUI.Models
{
    public class PageItem
    {
        public string Title { set; get; }
        public object ViewModel { set; get; }
        public string IconData { set; get; }

        public PageItem(string title,  object viewModel, string iconData = null)
        {
            Title = title;
            ViewModel = viewModel;
            IconData = iconData;
        }
    }
}
