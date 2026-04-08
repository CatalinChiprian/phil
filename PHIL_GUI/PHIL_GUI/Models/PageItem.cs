namespace PHIL_GUI.Models
{
    public class PageItem
    {
        public string Title { set; get; }
        public object ViewModel { set; get; }

        public PageItem(string title,  object viewModel)
        {
            Title = title;
            ViewModel = viewModel;
        }
    }
}
