using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace PHIL_GUI.Services;

public class SerialPortService
{
    private SerialPort serialPort;
    private bool isConnected;
    public string PortName { get; private set; }


    public event Action<string> MessageReceived;
    public event Action OnConnected;

    public List<string> GetAvailablePorts()
    {
        return SerialPort.GetPortNames().ToList(); // list availiable ports. If empty, check connection,
                                                   // check arduino lights and restart the program and computer. 
    }
    
    public void Connect(string portName, int baudRate = 9600)
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate)
            {
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                WriteTimeout = 200,

            };

            serialPort.DataReceived += OnDataReceived;
            
            serialPort.Open();
            isConnected = true;

            PortName = portName;

            OnConnected?.Invoke();
        }
        catch (Exception ex)
        {
            isConnected = false;
            throw new Exception($"Failed to connect: {ex.Message}");
        }
    }
    
    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            string data = serialPort.ReadLine();
            MessageReceived?.Invoke(data);
        }
        catch (Exception ex)
        {
            // read errors (timeout, etc.)
            MessageReceived?.Invoke($"Error reading: {ex.Message}");
        }
    }

    public async void SendMessage(string message)
    {
        if (!isConnected || serialPort == null || !serialPort.IsOpen) return;

        await Task.Run(() =>
        {
            try
            {
                serialPort.WriteLine(message);
            }
            catch (TimeoutException)
            {
                // device not responding - probably running GUI without PHIL.
            }
        });
    }
    
    public void Disconnect()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.DataReceived -= OnDataReceived;
            serialPort.Close();
        }
        isConnected = false;
    }
    
    public bool IsConnected => isConnected;
}