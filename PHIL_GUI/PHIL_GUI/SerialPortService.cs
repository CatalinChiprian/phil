using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace PHIL_GUI.Services;

public class SerialPortService
{
    private SerialPort _serialPort;
    private bool _isConnected;
    
    public event Action<string> MessageReceived;
    
    public List<string> GetAvailablePorts()
    {
        return SerialPort.GetPortNames().ToList(); // list availiable ports. If empty, check connection,
                                                   // check arduino lights and restart the program and computer. 
    }
    
    public void Connect(string portName, int baudRate = 9600) // choose the same baudrate in arduino IDE
    {
        try
        {
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            
            _serialPort.DataReceived += OnDataReceived;
            
            _serialPort.Open();
            _isConnected = true;
        }
        catch (Exception ex)
        {
            _isConnected = false;
            throw new Exception($"Failed to connect: {ex.Message}");
        }
    }
    
    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            string data = _serialPort.ReadLine();
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
        if (_isConnected && _serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.WriteLine(message);
        }
    }
    
    public void Disconnect()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.DataReceived -= OnDataReceived;
            _serialPort.Close();
        }
        _isConnected = false;
    }
    
    public bool IsConnected => _isConnected;
}