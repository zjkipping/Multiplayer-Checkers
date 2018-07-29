using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace MiddleManServer {
  public static class Server {
    private static Socket socket;
    private static int lobby_counter = 1;

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

        Console.WriteLine("Client Count: {0}", Clients.Count);
      }
    }

    public static void GetLobbyList(Client client) {
      Console.WriteLine("Getting Lobby List");
      string message = "LOBBY_LIST|";
      foreach (Lobby lobby in Lobbies) {
        message += lobby.ID + "-" + lobby.Host.Name + "-" + (int)lobby.Status + "-" + lobby.PlayerCount + ",";
      }
      Console.WriteLine(message);
      client.SendMessage(message);
    }

    public static void HostLobby(Client client, string name) {
      try {
        Console.WriteLine("Creating a Lobby");
        Lobby new_lobby = new Lobby(client, lobby_counter++, name);
        new_lobby.LobbyClosed += HandleLobbyClose;
        Lobbies.Add(new_lobby);
        client.SetType(ClientType.Host);
        client.Lobby = new_lobby;
        client.SendMessage("HOSTING|");
        Console.WriteLine("{0} is now hosting lobby with the id: {1}", client.IP, new_lobby.ID);
      } catch(Exception e) {
        Console.WriteLine(e);
      }
    }

    public static void JoinLobby(Client client, int id) {
      Lobby lobby = Lobbies.Find((Lobby l) => l.ID == id);
      if (lobby != null) {
        if (lobby.PlayerCount < 2 && lobby.ConnectingPlayer == null) {
          lobby.Connect(client);
        } else {
          client.SendMessage("ERROR|Lobby Is Full");
        }
      } else {
        client.SendMessage("ERROR|Lobby Doesn't Exist");
      }
    }

    public static void LeaveLobby(Client client) {
      try {
        if (client.Lobby != null) {
          client.Lobby.Player = null;
          client.Lobby = null;
        }
      } catch (Exception e) {
        Console.WriteLine(e);
      }
    }

    public static void CloseLobby(Client client) {
      try {
        Console.WriteLine(client.IP + " closed lobby with the id: " + client.Lobby.ID);
        client.SetType(ClientType.User);
        if (client.Lobby != null) {
          client.Lobby.Close(LobbyCloseReason.Clean);
          client.Lobby = null;
        }
      } catch (Exception e) {
        Console.WriteLine(e);
      }
    }

    public static void ConnectSuccess(Client client) {
      try {
        Console.WriteLine(client.Lobby.ConnectingPlayer.IP + " connected successfully to lobby: " + client.Lobby.ID);
        client.Lobby.Player = client.Lobby.ConnectingPlayer;
        client.Lobby.ConnectingPlayer = null;
        client.Lobby.Player.Lobby = client.Lobby;
      } catch (Exception e) {
        Console.WriteLine(e);
      }
    }

    public static void ConnectFail(Client client) {
      client.Lobby.ConnectingPlayer = null;
    }

    public static int GetClientCount() {
      return Clients.Count;
    }

    private static void HandleLobbyClose(Lobby sender, LobbyCloseEventArgs args) {
      Lobbies.Remove(sender);
    }

    private static void HandleClientDisconnect(Client sender, ClientDisconnectEventArgs args) {
      Clients.Remove(sender);
      if (sender.Lobby != null) {
        if (sender.Type == ClientType.Host) {
          sender.Lobby.Close(LobbyCloseReason.HostDisconnect);
        } else {
          sender.Lobby.Player = null;
        }
      }
    }
  }
}
