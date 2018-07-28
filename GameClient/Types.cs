using System.Windows;
using System.Collections.Generic;

namespace GameClient {
  public enum PlayerType {
    Player1,
    Player2
  }

  public enum PieceType {
    Normal,
    King
  }

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

  public class GamePiece {
    public Point Position;
    public PieceType Type;
    public PlayerType Owner;

    public GamePiece(Point position, PlayerType owner, PieceType type = PieceType.Normal) {
      Position = position;
      Owner = owner;
      Type = type;
    }
  }

  public class MoveOption {
    public Point Spot;
    public List<Point> HoppedPieces;

    public MoveOption(Point spot, List<Point> hoppedPieces) {
      Spot = spot;
      HoppedPieces = hoppedPieces;
    }
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
