namespace GameClient {
  public enum LobbyType {
    User,
    Host
  }

  public enum View {
    InitialLoading,
    MainMenu,
    LobbyList,
    LobbyLoading,
    LobbyScreen,
    GameScreen
  }

  public class LobbyListItem {
    private int id;
    public string ID {
      get {
        return id.ToString();
      }
    }

    private string hostName;
    public string HostName {
      get {
        return hostName;
      }
    }

    private LobbyStatus status;
    public string Status {
      get {
        return GetStatusText();
      }
    }

    private int playerCount;
    public string PlayerCount {
      get {
        return playerCount.ToString() + " / 2";
      }
    }

    public LobbyListItem(int id, string host_name, LobbyStatus status, int player_count) {
      this.id = id;
      hostName = host_name;
      this.status = status;
      playerCount = player_count;
    }

    private string GetStatusText() {
      switch (status) {
        case LobbyStatus.InGame:
          return "In-Game";
        case LobbyStatus.InLobby:
          return "In-Lobby";
        default:
          return "";
      }
    }
  }

  public enum LobbyStatus {
    InLobby,
    InGame
  }
}
