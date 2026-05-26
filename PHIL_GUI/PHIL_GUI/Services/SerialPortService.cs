using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace PHIL_GUI.Services;

public class SerialPortService
{
    private SerialPort serialPort;
    private bool isConnected;
    public string PortName { get; private set; }


    public event Action<string> MessageReceived;
    public event Action GetStartUpMessage;

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

            };

            serialPort.DataReceived += OnDataReceived;
            
            serialPort.Open();
            isConnected = true;

            PortName = portName;

            GetStartUpMessage?.Invoke();
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
    
    public void SendMessage(string message)
    {
        if (isConnected && serialPort != null && serialPort.IsOpen)
        {
            serialPort.WriteLine(message);
        }
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