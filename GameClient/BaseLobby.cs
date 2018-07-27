using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;

namespace GameClient {
  public class BaseLobby {
    public LobbyType Type;
    public Thread ListenThread = null;
    public Socket connection;
    public List<string> ChatMessages = new List<string>();
    public bool ListeningToConnection = false;

    public delegate void NewResponseEventHandler(string type, string parameters);
    public event NewResponseEventHandler NewResponse;

    public delegate void PeerDisconnectedEventHandler();
    public event PeerDisconnectedEventHandler PeerDisconnected;

    public delegate void NewMessageEventHandler(string message, string user);
    public event NewMessageEventHandler NewMessage;

    public BaseLobby(LobbyType type) {
      Type = type;
    }

    public void GotNewMessage(string message, string user = "") {
      ChatMessages.Add((user != "" ? user + ": " : "") + message);
      NewMessage?.Invoke(message, user);
    }

    public void SendMessage(string message) {
      try {
        connection.Send(Encoding.ASCII.GetBytes(message + "\r\n"));
      } catch (SocketException) {
        PeerDisconnected?.Invoke();
      }
    }

    public void Listen() {
      while (ListeningToConnection) {
        byte[] buffer = new byte[1024];
        try {
          string response = Encoding.ASCII.GetString(buffer, 0, connection.Receive(buffer));
          Console.WriteLine("RESPONSE: " + response);
          if (response == "") {
            PeerDisconnected?.Invoke();
            break;
          }
          response = response.Replace("\r\n", "");
          string[] sections = response.Split('|');
          NewResponse?.Invoke(sections[0], sections[1]);
        } catch {
          PeerDisconnected?.Invoke();
          break;
        }
      }
    }
  }
}
