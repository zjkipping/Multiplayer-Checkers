using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

using GameClient.Views;

namespace GameClient {
  class UserLobby: BaseLobby {
    public UserLobby(Socket connection) : base(LobbyType.User) {
      NewResponse += HandleResponse;

      this.connection = connection;
      ListenThread = new Thread(Listen);
      ListeningToConnection = true;
      ListenThread.Start();

      SendMessage("NAME|" + User.Name);
      User.Type = PlayerType.Player2;
    }

    public void SendChatMessage(string message) {
      SendMessage("MESSAGE|" + message + "-" + User.Name);
    }

    public void SelectTile(Point position) {
      SendMessage("TILE|" + position.ToString());
    }

    public void SelectPiece(Point position) {
      SendMessage("PIECE|" + position.ToString());
    }

    private void HandleResponse(string type, string parameters) {
      switch (type) {
        case "MESSAGE":
          string[] sections = parameters.Split('-');
          GotNewMessage(sections[0], sections[1]);
          break;
        case "STARTED":
          Application.Current.Dispatcher?.Invoke(() => ViewController.SetView(new GameView(this)));
          break;
        case "TURN":
          string[] turnSections = parameters.Split('-');
          if (int.TryParse(turnSections[0], out int playerNum)) {
            PlayerType player = (PlayerType)(playerNum - 1);
            List<GamePiece> forcedPieces = new List<GamePiece>();
            if (turnSections[1] != "") {
              new List<string>(turnSections[1].Split(':')).ForEach(delegate (string piece) {
                if (piece != "") {
                  try {
                    forcedPieces.Add(new GamePiece(Point.Parse(piece), player));
                  } catch (Exception e) {
                    Console.WriteLine(e);
                  }
                }
              });
            }
            StartNextTurn(player, forcedPieces);
          }
          break;
        case "PIECE_SELECTED":
          string[] pieceSections = parameters.Split('-');
          try {
            Point piece = Point.Parse(pieceSections[0]);
            List<MoveOption> options = new List<MoveOption>();
            if (pieceSections[1] != "") {
              new List<string>(pieceSections[1].Split('#')).ForEach(delegate (string option) {
                if (option != "") {
                  string[] optionParts = option.Split(':');
                  Point spot = Point.Parse(optionParts[0]);
                  List<Point> hoppedPieces = new List<Point>();
                  if (optionParts[1] != "") {
                    new List<string>(optionParts[1].Split('~')).ForEach(delegate (string hoppedPiece) {
                      if (hoppedPiece != "") {
                        hoppedPieces.Add(Point.Parse(hoppedPiece));
                      }
                    });
                  }
                  options.Add(new MoveOption(spot, hoppedPieces));
                }
              });
            }
            PieceSelect(piece, options);
          } catch(Exception e) {
            Console.WriteLine(e);
          }
          break;
        case "TILE_SELECTED":
          if (parameters != "") {
            string[] optionParts = parameters.Split(':');
            Point spot = Point.Parse(optionParts[0]);
            List<Point> hoppedPieces = new List<Point>();
            if (optionParts[1] != "") {
              new List<string>(optionParts[1].Split('~')).ForEach(delegate (string hoppedPiece) {
                if (hoppedPiece != "") {
                  hoppedPieces.Add(Point.Parse(hoppedPiece));
                }
              });
            }
            TileSelect(new MoveOption(spot, hoppedPieces));
          }
          break;
      }
    }
  }
}
