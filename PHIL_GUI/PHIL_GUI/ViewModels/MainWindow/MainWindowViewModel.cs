
/* Created by Catalin Chiprian
Based on Phillip Dettinger work availible on https://github.com/CSDGroup/PHIL.git */

using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using PHIL_GUI.Models;
using PHIL_GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace PHIL_GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        const string WellsIconData = "M168,949 C166.896,949 166,948.104 166,947 C166,945.896 166.896,945 168,945 C169.104,945 170,945.896 170,947 C170,948.104 169.104,949 168,949 L168,949 Z M168,943 C165.791,943 164,944.791 164,947 C164,949.209 165.791,951 168,951 C170.209,951 172,949.209 172,947 C172,944.791 170.209,943 168,943 L168,943 Z M168,959 C166.896,959 166,958.104 166,957 C166,955.896 166.896,955 168,955 C169.104,955 170,955.896 170,957 C170,958.104 169.104,959 168,959 L168,959 Z M168,953 C165.791,953 164,954.791 164,957 C164,959.209 165.791,961 168,961 C170.209,961 172,959.209 172,957 C172,954.791 170.209,953 168,953 L168,953 Z M168,939 C166.896,939 166,938.104 166,937 C166,935.896 166.896,935 168,935 C169.104,935 170,935.896 170,937 C170,938.104 169.104,939 168,939 L168,939 Z M168,933 C165.791,933 164,934.791 164,937 C164,939.209 165.791,941 168,941 C170.209,941 172,939.209 172,937 C172,934.791 170.209,933 168,933 L168,933 Z M180,949 C178.896,949 178,948.104 178,947 C178,945.896 178.896,945 180,945 C181.104,945 182,945.896 182,947 C182,948.104 181.104,949 180,949 L180,949 Z M180,943 C177.791,943 176,944.791 176,947 C176,949.209 177.791,951 180,951 C182.209,951 184,949.209 184,947 C184,944.791 182.209,943 180,943 L180,943 Z M156,939 C154.896,939 154,938.104 154,937 C154,935.896 154.896,935 156,935 C157.104,935 158,935.896 158,937 C158,938.104 157.104,939 156,939 L156,939 Z M156,933 C153.791,933 152,934.791 152,937 C152,939.209 153.791,941 156,941 C158.209,941 160,939.209 160,937 C160,934.791 158.209,933 156,933 L156,933 Z M180,959 C178.896,959 178,958.104 178,957 C178,955.896 178.896,955 180,955 C181.104,955 182,955.896 182,957 C182,958.104 181.104,959 180,959 L180,959 Z M180,953 C177.791,953 176,954.791 176,957 C176,959.209 177.791,961 180,961 C182.209,961 184,959.209 184,957 C184,954.791 182.209,953 180,953 L180,953 Z M156,959 C154.896,959 154,958.104 154,957 C154,955.896 154.896,955 156,955 C157.104,955 158,955.896 158,957 C158,958.104 157.104,959 156,959 L156,959 Z M156,953 C153.791,953 152,954.791 152,957 C152,959.209 153.791,961 156,961 C158.209,961 160,959.209 160,957 C160,954.791 158.209,953 156,953 L156,953 Z M180,935 C181.104,935 182,935.896 182,937 C182,938.104 181.104,939 180,939 C178.896,939 178,938.104 178,937 C178,935.896 178.896,935 180,935 L180,935 Z M180,941 C182.209,941 184,939.209 184,937 C184,934.791 182.209,933 180,933 C177.791,933 176,934.791 176,937 C176,939.209 177.791,941 180,941 L180,941 Z M156,949 C154.896,949 154,948.104 154,947 C154,945.896 154.896,945 156,945 C157.104,945 158,945.896 158,947 C158,948.104 157.104,949 156,949 L156,949 Z M156,943 C153.791,943 152,944.791 152,947 C152,949.209 153.791,951 156,951 C158.209,951 160,949.209 160,947 C160,944.791 158.209,943 156,943 L156,943 Z";
        const string CalibrationIconData = "M57.7,289 L54.55,289 L54.55,286 L52.45,286 L52.45,289 L49.3,289 L49.3,291 L52.45,291 L52.45,294 L54.55,294 L54.55,291 L57.7,291 L57.7,289 Z M55.6,280 L55.6,282 L61.9,282 L61.9,288 L64,288 L64,280 L55.6,280 Z M61.9,298 L55.6,298 L55.6,300 L64,300 L64,292 L61.9,292 L61.9,298 Z M45.1,292 L43,292 L43,300 L51.4,300 L51.4,298 L45.1,298 L45.1,292 Z M45.1,288 L43,288 L43,280 L51.4,280 L51.4,282 L45.1,282 L45.1,288 Z";
        const string MediumExchangeIconData = "M531 624l-88-1q18-44 51.5-76.5T572 500q55-18 110.5-3.5T778 554q4 5 10.5 5t11.5-4l26-24q5-5 5.5-12t-4.5-12q-53-58-127-77.5T553 433q-65 21-112.5 71.5T373 623h-93q-4 0-5 3t1 6l126 144q1 1 3 1t4-1l126-143q2-3 1-6t-5-3zm451 143L857 623q-2-2-4-2t-3 2L724 766q-3 2-1.5 5.5t4.5 3.5h88q-17 45-51 77.5T686 899q-55 18-110 3t-95-57q-5-5-11-5.5t-11 4.5l-26 23q-5 5-5.5 12t4.5 13q38 41 88.5 63.5T626 978q41 0 80-13 65-21 112.5-71T885 776h94q3 0 4.5-3.5T982 767zM70 252v447q0 14 9.5 23.5T103 732h127q6 0 11-4.5t5-11.5v-22q0-7-5-12t-11-5H125V296h568v56q0 7 4.5 12t11.5 5h21q7 0 12-5t5-12V252H70zm677-32v-55q0-13-9.5-23T714 132H613v-13q0-12-6-23t-16.5-17-22.5-6-22.5 6T529 96t-6 23v13H293v-13q0-19-13-32t-31.5-13T217 87t-13 32v13H103q-14 0-23.5 10T70 165v55h677z";


        public Action Disconnected;
        public ICommand DisconnectCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand GoHomeCommand { get; }
        public ICommand CalibrateHomeCommand { get; }
        public ICommand EmergencyStopCommand { get; }
        public List<PageItem> Pages { get; }
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

        public string ConnectedPort { get; }

        private object currentPage;
        public object CurrentPage
        {
            get => currentPage;
            set => SetProperty(ref currentPage, value);
        }

        public Well CurrentWell => RobotProtocolService.RobotState.CurrentWell;
        public LimitSwitches Limit => RobotProtocolService.RobotState.Limit;
        public RobotSettings Settings => RobotProtocolService.RobotState.Settings;
        public AppKeyBindings AppKeyBindings => AppSettingsService.AppSettings.AppKeyBindings;

        public MainWindowViewModel()
        {
            ConnectedPort = RobotProtocolService.SerialPort.PortName;

            Pages = new List<PageItem>
            {
                new PageItem("Wells", new WellsViewModel(), WellsIconData),
                new PageItem("Calibration", new CalibrationViewModel(), CalibrationIconData),
                new PageItem("Medium Exchange", new MediumExchangeViewModel(), MediumExchangeIconData)
            };

            SelectedPage = Pages[0];

            DisconnectCommand = new RelayCommand(Disconnect);
            MoveUpCommand = new RelayCommand(RobotProtocolService.MoveUp);
            MoveDownCommand = new RelayCommand(RobotProtocolService.MoveDown);
            GoHomeCommand = new RelayCommand(RobotProtocolService.GoHome);
            CalibrateHomeCommand = new RelayCommand(RobotProtocolService.CalibrateHome);
            EmergencyStopCommand = new RelayCommand(RobotProtocolService.EmergencyStop);
        }

        private void Disconnect()
        {
            RobotProtocolService.SerialPort.Disconnect();
            Disconnected?.Invoke();
        }
    }
}