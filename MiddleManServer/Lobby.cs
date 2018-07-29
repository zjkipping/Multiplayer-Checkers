using System;

namespace MiddleManServer {
  public class Lobby {
    public Client Host = null;
    public Client Player = null;
    public Client ConnectingPlayer = null;
    public int ID;
    public string Name;
    public LobbyStatus Status { get; private set; } = LobbyStatus.InLobby;
    public int PlayerCount {
      get {
        return (Host != null ? 1 : 0) + (Player != null ? 1 : 0);
      }
    }

    public event LobbyCloseEventHandler LobbyClosed;

    public Lobby(Client host, int id, string name) {
      Host = host;
      ID = id;
      Name = name;
    }

    public void Connect(Client client) {
      try {
        client.SendMessage("PUNCH-CLIENT|" + Host.IP.ToString());
        Host.SendMessage("PUNCH-HOST|" + client.IP.ToString());
        ConnectingPlayer = client;
      } catch (Exception e) {
        Console.WriteLine(e);
      }
    }

    public void SetStatus(int code) {
      Status = (LobbyStatus)code;
    }

    public void Close(LobbyCloseReason reason) {
      // stop other things if necessary, doubt it will be
      LobbyClosed?.Invoke(this, new LobbyCloseEventArgs(reason));
    }
  }
}
