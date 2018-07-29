using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameClient.Views {
  public partial class GameView : UserControl {
    PlayerType currentPlayer;
    BaseLobby Lobby = null;
    Board board = null;

    Point selectedPiece = new Point(-1, -1);
    List<GamePiece> currentForcedPieces = new List<GamePiece>();
    List<MoveOption> currentOptions = new List<MoveOption>();

    public GameView(BaseLobby lobby) {
      InitializeComponent();

      Lobby = lobby;
      ChatBox.ItemsSource = lobby.ChatMessages;
      lobby.NextTurn += Lobby_NextTurn;
      lobby.PieceSelected += Lobby_PieceSelected;
      lobby.TileSelected += Lobby_TileSelected;
      lobby.GameEnd += Lobby_GameEnd;
      lobby.PeerDisconnected += Lobby_PeerDisconnected;

      if (lobby.Type == LobbyType.Host) {
        MiddleManAPI.UpdateLobbyStatus(LobbyStatus.InLobby);
      }
    }

    private void Lobby_PeerDisconnected() {
      if (Lobby.Type == LobbyType.User) {
        (Lobby as UserLobby).Close();
        Dispatcher?.Invoke(() => ViewController.SetView(new LobbyListView()));
      } else if (Lobby.Type == LobbyType.Host) {
        (Lobby as HostLobby).Restart();
        Dispatcher?.Invoke(() => ViewController.SetView(new LobbyView(Lobby)));
      }
    }

    private void Lobby_GameEnd(PlayerType winner) {
      MessageBoxResult result = System.Windows.MessageBox.Show(winner == User.Type ? "Congratulations You Won!" : "Better Luck Next Time...",
                                          "Game Ended",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Information);
      if (result == MessageBoxResult.OK) {
        Dispatcher?.Invoke(() => {
          if (Lobby.Type == LobbyType.User) {
            (Lobby as UserLobby).Close();
          } else if (Lobby.Type == LobbyType.Host) {
            (Lobby as HostLobby).Close();
          }
          ViewController.SetView(new LobbyListView());
        });
      }
      // display some alert and send user back to lobby list
    }

    private void Lobby_TileSelected(bool kinged, MoveOption option) {
      Dispatcher?.Invoke(() => {
        // unhighlight everything from current players turn
        try {
          Console.WriteLine("Got Tile Selected: " + option.Spot+ " - " + option.HoppedPieces.Count);
          board.ResetPieceHighlight(selectedPiece);

          currentForcedPieces.ForEach(delegate (GamePiece piece) {
            board.ResetPieceHighlight(piece.Position);
          });

          currentOptions.ForEach(delegate (MoveOption moveOption) {
            board.ResetTileHighlight(moveOption.Spot);
            moveOption.HoppedPieces.ForEach(delegate (Point piece) {
              board.ResetPieceHighlight(piece);
            });
          });

          // move selected piece on board
          if (kinged) {
            board.KingPiece(selectedPiece);
          }
          board.MovePiece(selectedPiece, option.Spot);
          selectedPiece = new Point(-1, -1);

          // remove hopped pieces from board
          option.HoppedPieces.ForEach(delegate (Point piece) {
            board.RemovePiece(piece);
          });
        } catch(Exception e) {
          Console.WriteLine(e);
        }
      });
    }

    private void Lobby_PieceSelected(Point piece, List<MoveOption> options) {
      Dispatcher?.Invoke(() => {
        try {
          Console.WriteLine("Got Piece Selected: " + piece + " - " + options.Count);

          if (!selectedPiece.Equals(new Point(-1, -1))) {
            board.ResetPieceHighlight(selectedPiece);
          }

          if (currentForcedPieces.Count > 0) {
            currentForcedPieces.ForEach(delegate (GamePiece forced) {
              board.HighlightForcedPiece(forced.Position);
            });
          }

          selectedPiece = piece;
          board.HighlightSelectedPiece(piece);

          currentOptions.ForEach(delegate (MoveOption option) {
            board.ResetTileHighlight(option.Spot);
          });

          currentOptions = options;

          options.ForEach(delegate (MoveOption option) {
            board.HighlightTile(option.Spot);
          });
        } catch (Exception e) {
          Console.WriteLine(e);
        }
      });
    }

    private void Lobby_NextTurn(PlayerType player, List<GamePiece> forcedPieces) {
      Dispatcher?.Invoke(() => {
        try {
          Console.WriteLine("New Turn");
          Console.WriteLine("Got Next Turn: " + player + " - " + forcedPieces.Count);
          currentPlayer = player;
          SetCurrentPlayerText(player);
          currentForcedPieces = forcedPieces;

          forcedPieces.ForEach(delegate (GamePiece piece) {
            board.HighlightForcedPiece(piece.Position);
          });
        } catch (Exception e) {
          Console.WriteLine(e);
        }
      });
    }

    private void CanvasBoard_Loaded(object sender, RoutedEventArgs e) {
      board = new Board(CanvasBoard, Lobby.Type);

      board.TileClick += Board_TileClick;
      board.PieceClick += Board_PieceClick;
      board.TileHoverChange += Board_TileHoverChange;

      if (Lobby.Type == LobbyType.User) {
        (Lobby as UserLobby).SendMessage("READY|");
      } else if (Lobby.Type == LobbyType.Host) {
        (Lobby as HostLobby).SetReady();
      }
    }

    private void Board_TileHoverChange(Point spot, bool hovering) {
      MoveOption option = currentOptions.Find(o => o.Spot.Equals(spot));
      if (option != null && option.HoppedPieces.Count > 0) {
        option.HoppedPieces.ForEach(delegate (Point piece) {
          if (hovering) {
            board.HighlightHopPiece(piece);
          } else {
            board.ResetPieceHighlight(piece);
          }
        });
      }
    }

    private void Board_PieceClick(Point position) {
      if (User.Type == currentPlayer) {
        if (Lobby.Type == LobbyType.Host) {
          (Lobby as HostLobby).SelectPiece(position);
        } else if (Lobby.Type == LobbyType.User) {
          (Lobby as UserLobby).SelectPiece(position);
        }
      }
    }

    private void Board_TileClick(Point position) {
      if (User.Type == currentPlayer && !selectedPiece.Equals(new Point(-1, -1))) {
        if (Lobby.Type == LobbyType.Host) {
          (Lobby as HostLobby).SelectTile(position);
        } else if (Lobby.Type == LobbyType.User) {
          (Lobby as UserLobby).SelectTile(position);
        }
      }
    }

    private void SetCurrentPlayerText(PlayerType player) {
      string text = "Current Player: ";
      if (player == PlayerType.Player1) {
        text += Lobby.Player1;
      } else {
        text += Lobby.Player2;
      }
      CurrentPlayerText.Text = text;
    }

    private void LeaveButton_Click(object sender, RoutedEventArgs e) {
      if (Lobby.Type == LobbyType.User) {
        (Lobby as UserLobby).Close();
      } else if (Lobby.Type == LobbyType.Host) {
        (Lobby as HostLobby).Close();
      }
      Dispatcher?.Invoke(() => ViewController.SetView(new LobbyListView()));
    }

    private void ChatButton_Click(object sender, RoutedEventArgs e) {
      if (MessageBox.Text != "") {
        if (Lobby.Type == LobbyType.User) {
          (Lobby as UserLobby).SendChatMessage(MessageBox.Text);
          MessageBox.Text = "";
        } else if (Lobby.Type == LobbyType.Host) {
          (Lobby as HostLobby).SendChatMessage(MessageBox.Text);
          MessageBox.Text = "";
        }
      }
    }
  }
}
