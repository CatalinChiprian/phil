using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using PHIL_GUI.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

            //AppendLog();

            //if (message.StartsWith("POS:")) ParsePosition(message);
            //else if (message.StartsWith("CAL_COUNT:")) ParseCalCount(message);
            //else if (message.StartsWith("CAL_PT:")) ParseCalPoint(message);
            //else if (message.StartsWith("CAL_REC:")) ParseCalRecorded(message);
            //else if (message.StartsWith("CAL_ERR:")) ParseCalError(message);
            //else if (message.StartsWith("CAL_COEFFS_L:")) ParseCoeffsL(message);
            //else if (message.StartsWith("CAL_COEFFS_R:")) ParseCoeffsR(message);
            //else if (message.StartsWith("RMS:")) ParseRms(message);
            //else if (message.StartsWith("WELL:")) ParseWellArrival(message);
            //else if (message.StartsWith("LIMIT:")) ParseLimit(message);
            //else if (message.StartsWith("WARNING:")) ParseAlert(message, AlertLevel.Warning);
            //else if (message.StartsWith("ERROR:"))   ParseAlert(message, AlertLevel.Error);
        }

        protected void Send(string command)
        {
            SerialService.SendMessage(command);
        }

        //private Dictionary<string, string> ParseKV(string msg, string prefix)
        //{
        //    return msg.Substring(prefix.Length)
        //              .Split(',')
        //              .Select(p => p.Split('='))
        //              .Where(p => p.Length == 2)
        //              .ToDictionary(p => p[0], p => p[1]);
        //}

        //private void ParsePosition(string msg)
        //{
        //    var d = ParseKV(msg, "POS:");
        //    Dispatcher.UIThread.Post(() => {
        //        PositionL = long.Parse(d["L"]);
        //        PositionR = long.Parse(d["R"]);
        //        PositionZ1 = long.Parse(d["Z1"]);
        //        PositionZ2 = long.Parse(d["Z2"]);
        //    });
        //}

        //private void ParseRms(string msg)
        //{
        //    var d = ParseKV(msg, "RMS:");
        //    Dispatcher.UIThread.Post(() => {
        //        RmsL = double.Parse(d["L"], CultureInfo.InvariantCulture);
        //        RmsR = double.Parse(d["R"], CultureInfo.InvariantCulture);
        //        MaxErrL = double.Parse(d["MAX_L"], CultureInfo.InvariantCulture);
        //        MaxErrR = double.Parse(d["MAX_R"], CultureInfo.InvariantCulture);
        //    });
        //}

        //private void ParseCalPoint(string msg)
        //{
        //    // "CAL_PT:0,0.00,0.00,22.27,-74.02"
        //    var parts = msg.Substring("CAL_PT:".Length).Split(',');
        //    Dispatcher.UIThread.Post(() => {
        //        CalibrationPoints.Add(new CalibrationPoint
        //        {
        //            Index = int.Parse(parts[0]),
        //            X = double.Parse(parts[1], CultureInfo.InvariantCulture),
        //            Y = double.Parse(parts[2], CultureInfo.InvariantCulture),
        //            AngleL = double.Parse(parts[3], CultureInfo.InvariantCulture),
        //            AngleR = double.Parse(parts[4], CultureInfo.InvariantCulture),
        //        });
        //    });
        //}

        //private void ParseCalError(string msg)
        //{
        //    // "CAL_ERR:0,0.00,0.00,0.499,-0.296"
        //    var parts = msg.Substring("CAL_ERR:".Length).Split(',');
        //    int idx = int.Parse(parts[0]);
        //    var pt = CalibrationPoints.FirstOrDefault(p => p.Index == idx);
        //    if (pt != null) Dispatcher.UIThread.Post(() => {
        //        pt.ErrL = double.Parse(parts[3], CultureInfo.InvariantCulture);
        //        pt.ErrR = double.Parse(parts[4], CultureInfo.InvariantCulture);
        //    });
        //}


        //private void ParseAlert(string msg, AlertLevel level)
        //{
        //    // "WARNING:LIMIT_HIT,Z1=1,Z2=0"
        //    // "ERROR:HOME_FAILED,Homing timed out..."
        //    var body = msg.Substring(msg.IndexOf(':') + 1);
        //    var comma = body.IndexOf(',');

        //    var code = comma >= 0 ? body.Substring(0, comma) : body;
        //    var detail = comma >= 0 ? body.Substring(comma + 1) : "";

        //    Dispatcher.UIThread.Post(() => {
        //        Alerts.Insert(0, new RobotAlert
        //        {
        //            Level = level,
        //            Code = code,
        //            Message = detail
        //        });

        //        // Cap history
        //        while (Alerts.Count > 50) Alerts.RemoveAt(Alerts.Count - 1);

        //        // Surface the most recent alert for status bar binding
        //        LatestAlert = Alerts[0];
        //    });

        //public enum AlertLevel { Info, Warning, Error }

        //public class RobotAlert
        //{
        //    public AlertLevel Level { get; set; }
        //    public string Code { get; set; }  // e.g. "LIMIT_HIT"
        //    public string Message { get; set; }
        //    public DateTime Time { get; set; } = DateTime.Now;
        //}
    }
}
