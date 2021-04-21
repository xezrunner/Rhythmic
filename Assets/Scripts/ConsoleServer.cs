using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum CMessageType { Text = 0, DebugInfo = 1 }
public enum CLogType { None = -1, Info = 0, Unimportant = 1, Warning = 2, Error = 3, Caution = 4, Network = 5, IO = 6, Application = 7, UNKNOWN = 99 }

public static class ConsoleServer
{
    public static bool IsServerActive { get { return CServer != null && CServer.IsConnected; } }

    public static NamedPipeServerStream CServer;
    public static bool Stopping;

    public static void StartConsoleServer()
    {
        if (CServer != null) { Logger.LogWarning("CServer: CServer is already running!"); return; }
        Logger.Log("CServer: init!", CLogType.IO);

        Thread CServerThread = new Thread(() =>
        {
            CServer = new NamedPipeServerStream("RhythmicConsoleServer");
            CServer.WaitForConnection();


            // TODO: This does not work.
            // This should detect client disconnects and stop the server.
            /* 
            while (CServer != null && CServer.IsConnected)
                Thread.Sleep(1000);

            // If it isn't the game stopping the server, cause a server stop
            if (!Stopping) StopConsoleServer();
            */
        });
        CServerThread.Start();
        Logger.Log("CServer: Connected!", CLogType.IO);
    }
    public static void StopConsoleServer()
    {
        if (CServer == null) return;
        Logger.Log("CServer: closing server...", CLogType.IO);

        Stopping = true;

        if (!CServer.IsConnected)
            using (NamedPipeClientStream cs = new NamedPipeClientStream("RhythmicConsoleServer"))
                cs.Connect(1000);

        CServer.Close();
        CServer.Dispose();
        CServer = null;

        Logger.Log("CServer: server closed!", CLogType.IO);

        Stopping = false;
    }

    public static void Write(string text, CLogType logType = CLogType.Info, string color = null)
    {
        if (CServer == null)
            return;

        byte[] textBytes = Encoding.ASCII.GetBytes(text);
        byte[] colorBytes = new byte[0];

        bool isCustomColor = (color != null && color != "");
        if (isCustomColor)
            colorBytes = Encoding.ASCII.GetBytes(color);

        byte[] message = new byte[1024];

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
        CServer.Flush();
    }
}