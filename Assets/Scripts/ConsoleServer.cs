using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum CMessageType { Text }
public enum CLogType { Info = 0, Unimportant = 1, Warning = 2, Error = 3, Caution = 4, Network = 5, IO = 6, Application = 7, UNKNOWN = -1 }

public static class ConsoleServer
{
    public static NamedPipeServerStream CServer;

    public static void StartConsoleServer()
    {
        Task.Factory.StartNew(() =>
        {
            CServer = new NamedPipeServerStream("RhythmicConsoleServer");
            CServer.WaitForConnection();

            //WriteLine("Console Server started!");
        });

        Debug.Log("CServer init!");
    }

    public static void WriteLine(string text, CLogType logType = CLogType.Info, string color = null)
    {
        byte[] textBytes = Encoding.ASCII.GetBytes(text);
        byte[] colorBytes = new byte[0];

        bool isCustomColor = (color != null && color != "");
        if (isCustomColor)
            colorBytes = Encoding.ASCII.GetBytes(color);

        byte[] message = new byte[1024];

        // Padding: the first byte is seemingly consumed / doesn't arrive
        //message[0] = 0;
        // CMessageType : 1
        message[0] = (byte)CMessageType.Text;
        // CLogType : 1
        message[1] = (byte)logType;
        // Custom color length : ??
        message[2] = (byte)colorBytes.Length;
        // Text length : ??
        message[3] = (byte)textBytes.Length;

        int offset = 4;

        // Send custom color
        if (isCustomColor)
        {
            for (int i = 0; i < colorBytes.Length; i++)
                message[i + offset] = colorBytes[i];
            offset += colorBytes.Length;
        }

        // Send text
        for (int i = 0; i < textBytes.Length; i++)
            message[i + (offset)] = textBytes[i];

        int messageLength = 4 + colorBytes.Length + textBytes.Length;
        CServer.Write(message, 0, messageLength);
    }
}