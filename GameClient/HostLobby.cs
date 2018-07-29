using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Windows;

using GameClient.Views;

namespace GameClient {
  class HostLobby: BaseLobby {
    public delegate void StartEnabledEventHandler();
    public event StartEnabledEventHandler StartEnabled;
    public Game game = null;

    public HostLobby(): base(LobbyType.Host) {
      NewGame();
      User.Type = PlayerType.Player1;
      MiddleManAPI.UserConnected += (Socket connection) => {
        this.connection = connection;
        ListenThread = new Thread(Listen);
        ListeningToConnection = true;
        ListenThread.Start();
        GotNewMessage("---New User Connected---");
        StartEnabled.Invoke();
      };
      NewResponse += HandleResponse;
    }

    private void NewGame() {
      game = new Game(User.Name);
      game.NextTurn += Game_NextTurn;
      game.GameOver += Game_GameOver;
    }

    private void Game_NextTurn(PlayerType player, List<GamePiece> forcedPieces) {
      StartNextTurn(player, forcedPieces);
      string playerNum = player == PlayerType.Player1 ? "1" : "2";
      string forcedString = "";
      forcedPieces.ForEach(delegate (GamePiece piece) {
        forcedString += piece.Position.X + "," + piece.Position.Y + ":";
      });
      SendMessage("TURN|" + playerNum + "-" + forcedString);
    }

    private void Game_GameOver(PlayerType winner) {
      StartGameEnd(winner);
      SendMessage("GAME_END|" + (winner == PlayerType.Player1 ? 1 : 2));
    }

    public void StartGame() {
      SendMessage("STARTED|");
      ViewController.SetView(new GameView(this));
    }

    public void SendChatMessage(string message) {
      GotNewMessage(message, User.Name);
      SendMessage("MESSAGE|" + message + "-" + User.Name);
    }

    public void SelectTile(Point position) {
      (bool kinged, MoveOption option) = game.SelectTile(position);
      if (option != null) {
        TileSelect(kinged, option);
        string optionsString = "";
        optionsString += option.Spot.ToString() + ":";
        option.HoppedPieces.ForEach(delegate (Point piece) {
          optionsString += piece.ToString() + "~";
        });
        SendMessage("TILE_SELECTED|" + (kinged ? 1 : 0) + "-" + optionsString);
        game.StartNextTurn();
      } else {
        // invalid tile selection
      }
    }

    public void SelectPiece(Point position) {
      List<MoveOption> options = game.SelectPiece(position, PlayerType.Player1);
      if (options != null) {
        PieceSelect(position, options);
        string optionsString = "";
        options.ForEach(delegate (MoveOption option) {
          optionsString += option.Spot.ToString() + ":";
          option.HoppedPieces.ForEach(delegate (Point piece) {
            optionsString += piece.ToString() + "~";
          });
          optionsString += "#";
        });
        SendMessage("PIECE_SELECTED|" + position.ToString() + "-" + optionsString);
      } else {
        // TODO: dispatch event for bad selection?
      }
    }

    public void SetReady() {
      game.SetPlayerReady(PlayerType.Player1);
    }

    public void Restart() {
      ListeningToConnection = false;
      if (connection != null && connection.Connected) {
        connection.Close();
      }
      NewGame();
    }

    public void Close() {
      ListeningToConnection = false;
      if (connection != null && connection.Connected) {
        connection.Close();
      }
      MiddleManAPI.CloseLobby();
    }

    private void HandleResponse(string type, string parameters) {
      switch(type) {
        case "DISCONNECT":
          PeerDisconnect();
          break;
        case "MESSAGE":
          string[] parts = parameters.Split('-');
          string message = parts[0];
          string user = parts[1];
          GotNewMessage(message, user);
          SendMessage("MESSAGE|" + parameters);
          break;
        case "NAME":
          game.SetPlayer2Name(parameters);
          break;
        case "PIECE":
          try {
            Point position = Point.Parse(parameters);
            List<MoveOption> options = game.SelectPiece(position, PlayerType.Player2);
            if (options != null) {
              PieceSelect(position, options);
              string optionsString = "";
              options.ForEach(delegate (MoveOption option) {
                optionsString += option.Spot.ToString() + ":";
                option.HoppedPieces.ForEach(delegate (Point piece) {
                  optionsString += piece.ToString() + "~";
                });
                optionsString += "#";
              });
              SendMessage("PIECE_SELECTED|" + position.ToString() + "-" + optionsString);
            }
          } catch (Exception e) {
            Console.WriteLine(e);
          }
          break;
        case "TILE":
          try {
            (bool kinged, MoveOption option) = game.SelectTile(Point.Parse(parameters));
            if (option != null) {
              TileSelect(kinged, option);
              string optionsString = "";
              optionsString += option.Spot.ToString() + ":";
              option.HoppedPieces.ForEach(delegate (Point piece) {
                optionsString += piece.ToString() + "~";
              });
              SendMessage("TILE_SELECTED|" + (kinged ? 1 : 0) + "-" + optionsString);
              game.StartNextTurn();
            }
          } catch (Exception e) {
            Console.WriteLine(e);
          }
          break;
        case "READY":
          game.SetPlayerReady(PlayerType.Player2);
          break;
      }
    }
  }
}
