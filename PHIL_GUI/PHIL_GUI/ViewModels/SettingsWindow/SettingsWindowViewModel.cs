using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System.Collections.Generic;

namespace PHIL_GUI.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        private PageItem selectedPage;
        public PageItem SelectedPage
        {
            get => selectedPage;
            set
            {
                SetProperty(ref selectedPage, value);

                CurrentPage = value.ViewModel;
            }
        }

        public PageItem DebugPage { get; }

        private object currentPage;
        public object CurrentPage
        {
            get => currentPage;
            set => SetProperty(ref currentPage, value);
        }

        public SettingsWindowViewModel()
        {
            DebugPage = new PageItem("Debug", null); // Change VM
        }
    }
}
