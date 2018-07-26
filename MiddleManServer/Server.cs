using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace MiddleManServer {
  public static class Server {
    private static Socket socket;
    private static int lobby_counter = 0;

    private static List<Client> Clients = new List<Client>();
    private static List<Lobby> Lobbies = new List<Lobby>();

    private static bool AcceptingClients;
    private static Thread AcceptClientsThread;

    public const int Port = 5000;
    public static EndPoint IP;

    public static bool Start() {
      try {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), Port);
        socket.Bind(IP);
        socket.Listen(1);

        AcceptClientsThread = new Thread(() => AcceptClients());
        AcceptingClients = true;
        AcceptClientsThread.Start();

        return true;
      } catch {
        return false;
      }
    }

    public static void AcceptClients() {
      while (AcceptingClients) {
        Socket new_socket = socket.Accept();
        Client client = new Client(new_socket);
        Clients.Add(client);
        client.Start();
        client.ClientDisconnected += HandleClientDisconnect;

        Console.WriteLine("Got new client!");
        Console.WriteLine("Client Count: {0}", Clients.Count);
      }
    }

    public static void GetLobbyList(Client client) {
      Console.WriteLine("Getting Lobby List");
      string message = "LOBBY_LIST|";
      foreach (Lobby lobby in Lobbies) {
        message += lobby.ID + "-" + lobby.Host.Name + "-" + lobby.Status + "-" + lobby.PlayerCount + ",";
      }
      client.SendMessage(message);
    }

    public static void HostLobby(Client client, string name) {
      Lobby new_lobby = new Lobby(client, lobby_counter++, name);
      new_lobby.LobbyClosed += HandleLobbyClose;
      Lobbies.Add(new_lobby);
      Console.WriteLine("{0} is now hosting lobby with the id: {1}", client.IP, new_lobby.ID);
    }

    public static void JoinLobby(Client client, int id) {
      Lobby lobby = Lobbies.Find((Lobby l) => l.ID == id);
      if (lobby != null) {
        lobby.Connect(client);
      } else {
        client.SendMessage("ERROR|Lobby Doesn't Exist");
      }
    }

    public static void LeaveLobby(Client client) {
      client.Lobby.Player = null;
      client.Lobby = null;
    }

    public static void CloseLobby(Client client) {
      client.Lobby.Close(LobbyCloseReason.Clean);
      client.Lobby = null;
    }

    public static int GetClientCount() {
      return Clients.Count;
    }

    private static void HandleLobbyClose(Lobby sender, LobbyCloseEventArgs args) {
      Lobbies.Remove(sender);
    }

    private static void HandleClientDisconnect(Client sender, ClientDisconnectEventArgs args) {
      Clients.Remove(sender);
      Lobby hostedLobby = Lobbies.Find((Lobby l) => l.Host == sender);
      if (hostedLobby != null) {
        hostedLobby.Close(LobbyCloseReason.HostDisconnect);
      }
    }
  }
}
