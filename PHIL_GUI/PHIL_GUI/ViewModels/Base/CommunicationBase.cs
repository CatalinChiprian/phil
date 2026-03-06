using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.Services;
using System;

namespace PHIL_GUI.ViewModels.Base
{
    public abstract class CommunicationBase : ViewModelBase
    {
        protected readonly SerialPortService SerialService;

        private string _receivedData = "";
        public string ReceivedData
        {
            get => _receivedData;
            private set => SetProperty(ref _receivedData, value);
        }

        protected CommunicationBase()
        {
            SerialService = App.Services.GetRequiredService<SerialPortService>();
            SerialService.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(string message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ReceivedData += $"{DateTime.Now:HH:mm:ss}: {message}\n";
            });
        }

        protected void Send(string command)
        {
            SerialService.SendMessage(command);
        }
    }
}
