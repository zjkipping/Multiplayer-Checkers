using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace GameClient {
  public static class MiddleManAPI {
    private static Socket socket = null;
    private static IPEndPoint IP = new IPEndPoint(IPAddress.Parse("18.212.35.145"), 5000); // IP/Port for the middleman server
    private static int HostPort = 12344; // port used when hosting a lobby
    private static Thread ResponseThread = null;
    private static bool ResponseThreadRunning = false;

    public delegate void ConnectedSuccessEventHandler();
    public static event ConnectedSuccessEventHandler ConnectedSuccess;

    public delegate void NewLobbyListEventHandler(List<LobbyListItem> list);
    public static event NewLobbyListEventHandler NewLobbyList;

    public delegate void LobbyCreatedEventHandler();
    public static event LobbyCreatedEventHandler LobbyCreated;

    public delegate void JoinLobbySuccessEventHandler(Socket connection);
    public static event JoinLobbySuccessEventHandler JoinLobbySuccess;

    public delegate void JoinLobbyFailureEventHandler();
    public static event JoinLobbyFailureEventHandler JoinLobbyFailure;

    public delegate void UserConnectedEventHandler(Socket connection);
    public static event UserConnectedEventHandler UserConnected;

    public static bool Connect() {
      if (socket == null) {
        try {
          socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
          socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

          socket.Bind(new IPEndPoint(IPAddress.Any, getUsablePort()));
          socket.Connect(IP);
        } catch {
          socket = null;
          return false;
        }

        ResponseThread = new Thread(Responses);
        ResponseThreadRunning = true;
        ResponseThread.Start();

        return true;
      } else {
        return false;
      }
    }

    public static void RequestLobbyList() {
      SendMessage("REQ_LOBBIES|");
    }

    public static void HostLobby() {
      SendMessage("HOST|");
    }

    public static void JoinLobby(int id) {
      SendMessage("CONNECT|" + id);
    }

    private static int getUsablePort() {
      Console.WriteLine("Testing Port: " + HostPort);
      while (PortInUse(HostPort++)) { }
      Console.WriteLine("Found Port: " + HostPort);
      return HostPort;
    }

    private static bool PortInUse(int port) {
      bool inUse = false;
      IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
      IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
      foreach (IPEndPoint endPoint in ipEndPoints) {
        if (endPoint.Port == port) {
          inUse = true;
          break;
        }
      }
      return inUse;
    }

    private static void Responses() {
      while (ResponseThreadRunning) {
        // listens to responses from the middle man server
        string response = GetResponse();
        if (response != "") {
          string[] sections = response.Split('|');
          string responseType = sections[0];
          string parameters = sections[1].Replace("\r", "").Replace("\n", "");

          Console.WriteLine(responseType + " | " + parameters);

          switch (responseType) {
            case "CONNECTED":
              User.Name = parameters;
              ConnectedSuccess?.Invoke();
              break;
            case "LOBBY_LIST":
              string[] lobbies = parameters.Split(',');
              List<LobbyListItem> lobbyList = new List<LobbyListItem>();
              foreach(string lobby in lobbies) {
                string[] info = lobby.Split('-');
                if (int.TryParse(info[0], out int id) && int.TryParse(info[2], out int status) && int.TryParse(info[3], out int player_count)) {
                  lobbyList.Add(new LobbyListItem(id, info[1], (LobbyStatus)status, player_count));
                }
              }
              NewLobbyList?.Invoke(lobbyList);
              break;
            case "HOSTING":
              LobbyCreated?.Invoke();
              break;
            case "PUNCH-HOST":
              Socket connectionToC = PerformPunchThrough(sections[1].Split(':'));
              if (connectionToC != null) {
                UserConnected?.Invoke(connectionToC);
              }
              break;
            case "PUNCH-CLIENT":
              Socket connectionToH = PerformPunchThrough(sections[1].Split(':'));
              if (connectionToH != null) {
                JoinLobbySuccess?.Invoke(connectionToH);
              } else {
                JoinLobbyFailure.Invoke();
              }
              break;
          }

        } else {
          // raise disconnect events
          Console.WriteLine("Server Connection Failed...");
          break;
        }
      }
    }

    private static string GetResponse() {
      try {
        byte[] buffer = new byte[1024];
        return Encoding.ASCII.GetString(buffer, 0, socket.Receive(buffer));
      } catch {
        return "";
      }
    }

    private static void SendMessage(string message) {
      try {
        socket.Send(Encoding.ASCII.GetBytes(message + "\r\n"));
      } catch (SocketException) {
        // disconnected from middle man server, do something
      }
    }

    private static Socket PerformPunchThrough(string[] peerInfo) {
      IPEndPoint peer = new IPEndPoint(IPAddress.Parse(peerInfo[0]), int.Parse(peerInfo[1]));
      Socket client = null;
      bool connected = false;
      int attempts = 0;
      while (!connected && attempts <= 25) {
        try {
          if (client != null) {
            client.Close();
          }
          client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
          client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
          IPEndPoint mine = new IPEndPoint(IPAddress.Any, HostPort);
          client.Bind(mine);
          Console.WriteLine("Mine: " + mine + "   Peer:   " + peer);
          client.Connect(peer);
          connected = true;
        } catch (Exception e) {
          Console.WriteLine(e);
        }
        attempts++;
      }

      if (connected) {
        return client;
      } else {
        return null;
      }
    }
  }
}
