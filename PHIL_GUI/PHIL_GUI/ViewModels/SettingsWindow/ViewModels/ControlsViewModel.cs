using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;

namespace PHIL_GUI.ViewModels
{
    public class ControlsViewModel : ViewModelBase, ISettingsPage
    {
        private AppKeyBindings appKeyBindings;
        public AppKeyBindings AppKeyBindings
        {
            get => appKeyBindings;
            set => SetProperty(ref appKeyBindings, value);
        }

        public ControlsViewModel()
        {
            AppKeyBindings = new AppKeyBindings(AppSettingsService.AppSettings.AppKeyBindings);
        }

        public void ApplyChanges()
        {
            AppSettingsService.AppSettings.AppKeyBindings = AppKeyBindings;
        }
    }
}
