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
      game = new Game(User.Name);
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

      game.NextTurn += (PlayerType player, List<GamePiece> forcedPieces) => {
        StartNextTurn(player, forcedPieces);
        string playerNum = player == PlayerType.Player1 ? "1" : "2";
        string forcedString = "";
        forcedPieces.ForEach(delegate (GamePiece piece) {
          forcedString += piece.Position.X + "," + piece.Position.Y + ":";
        });
        SendMessage("TURN|" + playerNum + "-" + forcedString);
      };
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
        TileSelect(option);
        string optionsString = "";
        optionsString += option.Spot.ToString() + ":";
        option.HoppedPieces.ForEach(delegate (Point piece) {
          optionsString += piece.ToString() + "~";
        });
        SendMessage("TILE_SELECTED|" + optionsString);
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

    private void HandleResponse(string type, string parameters) {
      switch(type) {
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
        case "PIECE": // when user selects a piece
          try {
            Point position = Point.Parse(parameters);
            List<MoveOption> options = game.SelectPiece(position, PlayerType.Player2);
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
          } catch (Exception e) {
            Console.WriteLine(e);
          }
          break;
        case "TILE":
          try {
            (bool kinged, MoveOption option) = game.SelectTile(Point.Parse(parameters));
            if (option != null) {
              TileSelect(option);
              string optionsString = "";
              optionsString += option.Spot.ToString() + ":";
              option.HoppedPieces.ForEach(delegate (Point piece) {
                optionsString += piece.ToString() + "~";
              });
              SendMessage("TILE_SELECTED|" + optionsString);
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
