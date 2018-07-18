using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace GameClient {
  public delegate void ResponseParsedHandler(ResponseParsedEventArgs args);

  public class ResponseParsedEventArgs: EventArgs {
    public MiddleManResponse response;
    public ResponseParsedEventArgs(MiddleManResponse value) {
      response = value;
    }
  }

  public static class MiddleManAPI {
    private static Socket socket;
    private static IPEndPoint IP = new IPEndPoint(IPAddress.Parse("54.197.196.186"), 5000); // IP/Port for the middleman server
    private const int HostPort = 4321; // port used when hosting a lobby
    private static Thread ResponseThread = null;
    private static bool ResponseThreadRunning = false;

    public static bool Connected { get { return socket.Connected; } }

    public static event ResponseParsedHandler ResponseParsed;
      
    public static bool Connect() {
      try {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        socket.Bind(new IPEndPoint(IPAddress.Any, HostPort));
        socket.Connect(IP);
      } catch {
        return false;
      }

      ResponseThread = new Thread(Responses);
      ResponseThreadRunning = true;
      ResponseThread.Start();

      return true;
    }

    private static void Responses() {
      while (ResponseThreadRunning) {
         // listens to responses from the middle man server
      }
    }
  }
}
