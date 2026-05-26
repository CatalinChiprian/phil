using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.Services;
using PHIL_GUI.ViewModels.Base;
using System.Collections.Generic;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        const string PlateIconData = "M 7.7148 49.5742 L 48.2852 49.5742 C 53.1836 49.5742 55.6446 47.1367 55.6446 42.3086 L 55.6446 13.6914 C 55.6446 8.8633 53.1836 6.4258 48.2852 6.4258 L 7.7148 6.4258 C 2.8398 6.4258 .3554 8.8398 .3554 13.6914 L .3554 42.3086 C .3554 47.1602 2.8398 49.5742 7.7148 49.5742 Z M 7.7851 45.8008 C 5.4413 45.8008 4.1288 44.5586 4.1288 42.1211 L 4.1288 13.8789 C 4.1288 11.4414 5.4413 10.1992 7.7851 10.1992 L 48.2147 10.1992 C 50.5350 10.1992 51.8708 11.4414 51.8708 13.8789 L 51.8708 42.1211 C 51.8708 44.5586 50.5350 45.8008 48.2147 45.8008 Z";
        const string ControlsIconData = "M5,13H7a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H5a.9.9,0,0,1-1-1V14A.9.9,0,0,1,5,13Zm6,0h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H11a.9.9,0,0,1-1-1V14A.9.9,0,0,1,11,13Zm6,0h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H17a.9.9,0,0,1-1-1V14A.9.9,0,0,1,17,13Zm6,0h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H23a.9.9,0,0,1-1-1V14A.9.9,0,0,1,23,13Zm6,0h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H29a.9.9,0,0,1-1-1V14A.9.9,0,0,1,29,13Zm6,0h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H35a.9.9,0,0,1-1-1V14A.9.9,0,0,1,35,13Zm6,0h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H41a.9.9,0,0,1-1-1V14A.9.9,0,0,1,41,13ZM11,19h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H11a.9.9,0,0,1-1-1V20A.9.9,0,0,1,11,19Zm6,0h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H17a.9.9,0,0,1-1-1V20A.9.9,0,0,1,17,19Zm6,0h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H23a.9.9,0,0,1-1-1V20A.9.9,0,0,1,23,19Zm6,0h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H29a.9.9,0,0,1-1-1V20A.9.9,0,0,1,29,19Zm6,0h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H35a.9.9,0,0,1-1-1V20A.9.9,0,0,1,35,19ZM17,25h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H17a.9.9,0,0,1-1-1V26A.9.9,0,0,1,17,25Zm6,0h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H23a.9.9,0,0,1-1-1V26A.9.9,0,0,1,23,25Zm6,0h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H29a.9.9,0,0,1-1-1V26A.9.9,0,0,1,29,25Zm0,0h2a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H29a.9.9,0,0,1-1-1V26A.9.9,0,0,1,29,25ZM9,31H39a.9.9,0,0,1,1,1v2a.9.9,0,0,1-1,1H9a.9.9,0,0,1-1-1V32A.9.9,0,0,1,9,31Z";
        const string DebugIconData = "M4.6 15c-.9-2.6-.6-4.6-.5-5.4 2.4-1.5 5.3-2 8-1.3.7-.3 1.5-.5 2.3-.6-.1-.3-.2-.5-.3-.8h2l1.2-3.2-.9-.4-1 2.6h-1.8C13 4.8 12.1 4 11.1 3.4l2.1-2.1-.7-.7L10.1 3c-.7 0-1.5 0-2.3.1L5.4.7l-.7.7 2.1 2.1C5.7 4.1 4.9 4.9 4.3 6H2.5l-1-2.6-.9.4L1.8 7h2C3.3 8.3 3 9.6 3 11H1v1h2c0 1 .2 2 .5 3H1.8L.6 18.3l.9.3 1-2.7h1.4c.4.8 2.1 4.5 5.8 3.9-.3-.2-.5-.5-.7-.8-2.9 0-4.4-3.5-4.4-4zM9 3.9c2 0 3.7 1.6 4.4 3.8-2.9-1-6.2-.8-9 .6.7-2.6 2.5-4.4 4.6-4.4zm14.8 19.2l-4.3-4.3c2.1-2.5 1.8-6.3-.7-8.4s-6.3-1.8-8.4.7-1.8 6.3.7 8.4c2.2 1.9 5.4 1.9 7.7 0l4.3 4.3c.2.2.5.2.7 0 .2-.2.2-.5 0-.7zm-8.8-3c-2.8 0-5.1-2.3-5.1-5.1s2.3-5.1 5.1-5.1 5.1 2.3 5.1 5.1-2.3 5.1-5.1 5.1z";

        public ICommand CancelCommand { get; set; }
        public ICommand SaveChangesCommand { get; set; }

        private PageItem selectedPage;
        public PageItem SelectedPage
        {
            get => selectedPage;
            set
            {
                if (value == null) return;

                SetProperty(ref selectedPage, value);

                CurrentPage = value.ViewModel;
            }
        }

        public List<PageItem> GeneralPages { get; }
        public List<PageItem> DeveloperPages { get; }
        public List<PageItem> AllPages
        {
            get
            {
                var allPages = new List<PageItem>();
                allPages.AddRange(GeneralPages);
                allPages.AddRange(DeveloperPages);
                return allPages;
            }
        }

        private object currentPage;
        public object CurrentPage
        {
            get => currentPage;
            set => SetProperty(ref currentPage, value);
        }

        public SettingsWindowViewModel()
        {
            SaveChangesCommand = new RelayCommand(Save);

            GeneralPages = new List<PageItem>()
            {
                new PageItem("Plate", new PlateViewModel(), PlateIconData),
                new PageItem("Controls", new ControlsViewModel(), ControlsIconData),
            };

            DeveloperPages = new List<PageItem>()
            {
                new PageItem("Debug", new DebugViewModel(), DebugIconData),
            };

            SelectedPage = AllPages[0];
        }

        private void Save()
        {
            foreach (PageItem page in AllPages)
            {
                if (page.ViewModel is ISettingsPage settingsPage)
                {
                    settingsPage.ApplyChanges();
                }
            }

            AppSettingsService.Save();
        }
    }
}
