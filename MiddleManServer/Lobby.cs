namespace MiddleManServer {
  public class Lobby {
    public Client Host = null;
    public Client Player = null;
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
      Player = client;
      Player.SendMessage("PUNCH-CLIENT|" + Host.IP.ToString());
      Host.SendMessage("PUNCH-HOST|" + Player.IP.ToString());
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
