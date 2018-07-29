using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;

namespace GameClient {
  public class BaseLobby {
    public LobbyType Type;
    public Thread ListenThread = null;
    public Socket connection = null;
    public ObservableCollection<string> ChatMessages = new ObservableCollection<string>();
    public bool ListeningToConnection = false;

    public string Player1 = "Host";
    public string Player2 = "User";

    public delegate void PieceSelectedEventHandler(Point piece, List<MoveOption> options);
    public event PieceSelectedEventHandler PieceSelected;

    public delegate void TileSelectedEventHandler(bool kinged, MoveOption option);
    public event TileSelectedEventHandler TileSelected;

    public delegate void NextTurnEventHandler(PlayerType player, List<GamePiece> forcedPieces);
    public event NextTurnEventHandler NextTurn;

    public delegate void GameEndEventHandler(PlayerType winner);
    public event GameEndEventHandler GameEnd;

    public delegate void NewResponseEventHandler(string type, string parameters);
    public event NewResponseEventHandler NewResponse;

    public delegate void PeerDisconnectedEventHandler();
    public event PeerDisconnectedEventHandler PeerDisconnected;

    public BaseLobby(LobbyType type) {
      Type = type;
    }

    public void GotNewMessage(string message, string user = "") {
      Console.WriteLine("wtf");
      Application.Current.Dispatcher?.Invoke(() => ChatMessages.Add((user != "" ? user + ": " : "") + message));
    }

    public void StartNextTurn(PlayerType player, List<GamePiece> forcedPieces) {
      NextTurn?.Invoke(player, forcedPieces);
    }

    public void PeerDisconnect() {
      PeerDisconnected?.Invoke();
  }

    public void StartGameEnd(PlayerType winner) {
      GameEnd?.Invoke(winner);
    }

    public void PieceSelect(Point piece, List<MoveOption> options) {
      PieceSelected?.Invoke(piece, options);
    }

    public void TileSelect(bool kinged, MoveOption option) {
      TileSelected?.Invoke(kinged, option);
    }

    public void SendMessage(string message) {
      if (connection != null) {
        try {
          connection.Send(Encoding.ASCII.GetBytes(message + "\r\n"));
          Console.WriteLine("SENT MESSAGE:   " + message);
        } catch (Exception e) {
          Console.WriteLine(e);
          PeerDisconnected?.Invoke();
        }
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
