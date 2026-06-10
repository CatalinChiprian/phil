using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHIL_GUI.Services;

/// <summary>
/// Service for managing serial port communication with the robot hardware.
/// Handles connection, disconnection, message sending/receiving, and event notifications.
/// </summary>
public class SerialPortService
{
    private SerialPort serialPort;
    private bool isConnected;
    /// <summary>
    /// Gets the name of the currently connected port.
    /// </summary>
    public string PortName { get; private set; }

    private StringBuilder buffer = new StringBuilder();

    /// <summary>
    /// Event raised when a complete message is received from the serial port.
    /// </summary>
    public event Action<string> MessageReceived;
    /// <summary>
    /// Event raised when the serial port connection is successfully established.
    /// </summary>
    public event Action OnConnected;

    /// <summary>
    /// Gets a list of available serial port names on the system.
    /// </summary>
    /// <returns>List of available serial port names.</returns>
    public List<string> GetAvailablePorts()
    {
        return SerialPort.GetPortNames().ToList(); // list availiable ports. If empty, check connection,
                                                   // check arduino lights and restart the program and computer. 
    }

    /// <summary>
    /// Connects to the specified serial port with the given baud rate.
    /// </summary>
    /// <param name="portName">The name of the port to connect to (e.g., "COM3").</param>
    /// <param name="baudRate">The baud rate for communication (default: 9600).</param>
    /// <exception cref="Exception">Thrown when connection fails.</exception>
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
            string chunk = serialPort.ReadExisting();
            buffer.Append(chunk);

            int newlineIndex;

            while ((newlineIndex = buffer.ToString().IndexOf('\n')) >= 0)
            {
                string line = buffer.ToString(0, newlineIndex).Trim();
                buffer.Remove(0, newlineIndex + 1);

                MessageReceived?.Invoke(line);
            }

        }
        catch (Exception ex)
        {
            MessageReceived?.Invoke($"Error reading: {ex.Message}");
        }
    }

    /// <summary>
    /// Sends a message to the connected serial port asynchronously.
    /// Message is sent with a newline terminator.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void SendMessage(string message)
    {
        if (!isConnected || serialPort == null || !serialPort.IsOpen) return;

        Task.Run(() =>
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

    /// <summary>
    /// Disconnects from the currently connected serial port.
    /// </summary>
    public void Disconnect()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.DataReceived -= OnDataReceived;
            serialPort.Close();
        }
        isConnected = false;
    }

    /// <summary>
    /// Gets whether the serial port is currently connected.
    /// </summary>
    public bool IsConnected => isConnected;
}