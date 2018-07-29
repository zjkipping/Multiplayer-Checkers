using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Windows;

namespace GameClient {
  public class Game {
    public string Player1 = "";
    public string Player2 = "";
    public bool Player1Ready = false;
    public bool Player2Ready = false;
    public bool Started = false;
    public bool ForcedMoves = true;

    public delegate void NextTurnEventHandler(PlayerType player, List<GamePiece> forcedPieces);
    public event NextTurnEventHandler NextTurn;

    public delegate void GameOverEventHandler(PlayerType winner);
    public event GameOverEventHandler GameOver;

    private List<Point> Player1Movement = new List<Point>(new Point[] { new Point(-1, -1), new Point(1, -1) });
    private List<Point> Player2Movement = new List<Point>(new Point[] { new Point(-1, 1), new Point(1, 1) });
    private List<Point> KingMovement = new List<Point>(new Point[] { new Point(-1, -1), new Point(1, -1), new Point(-1, 1), new Point(1, 1) });

    private List<GamePiece> pieces = new List<GamePiece>();
    private GamePiece selectedPiece = null;
    private List<GamePiece> forcedPieces = new List<GamePiece>();
    private List<MoveOption> currentOptions = new List<MoveOption>();

    private PlayerType currentPlayer;

    public Game(string player1) {
      Player1 = player1;

      // setup the pieces
      for (int y = 0; y < 3; y++) {
        for (int x = 0; x < 8; x++) {
          if (((x + 1) + y) % 2 == 0) {
            GamePiece new_piece = new GamePiece(new Point(x, y), PlayerType.Player2);
            pieces.Add(new_piece);
          }
        }
      }
      for (int y = 7; y > 4; y--) {
        for (int x = 7; x >= 0; x--) {
          if (((x + 1) + y) % 2 == 0) {
            GamePiece new_piece = new GamePiece(new Point(x, y), PlayerType.Player1);
            pieces.Add(new_piece);
          }
        }
      }
    }

    public void SetPlayer2Name(string name) {
      Player2 = name;
    }

    public void SetPlayerReady(PlayerType player) {
      if (player == PlayerType.Player1) {
        Player1Ready = true;
      } else if (player == PlayerType.Player2) {
        Player2Ready = true;
      }

      if (Player1Ready && Player2Ready) {
        StartNextTurn();
      }
    }

    public (bool, MoveOption) SelectTile(Point position) {
      MoveOption option = currentOptions.Find(o => o.Spot.Equals(position));
      if (option != null) {
        if (selectedPiece != null) {
          bool kinged = (selectedPiece.Owner == PlayerType.Player1 && option.Spot.Y == 0) || (selectedPiece.Owner == PlayerType.Player2 && option.Spot.Y == 7);
          selectedPiece.Position = option.Spot;
          if (kinged) {
            selectedPiece.Type = PieceType.King;
          }
          option.HoppedPieces.ForEach(delegate (Point hoppedPoint) {
            pieces.RemoveAll(p => p.Position.Equals(hoppedPoint));
          });
          selectedPiece = null;
          return (kinged, option);
        } else {
          // no piece in that spot (probably will never happen)
          return (false, null);
        }
      } else {
        // invalid tile
        return (false, null);
      }
    }

    public List<MoveOption> SelectPiece(Point position, PlayerType player) {
      List<MoveOption>oldOptions = new List<MoveOption>(currentOptions);
      currentOptions = new List<MoveOption>();
      GamePiece piece = pieces.Find(p => p.Position.Equals(position));
      if (piece != null && piece.Owner == player && (forcedPieces.Count == 0 || (forcedPieces.Count > 0 && forcedPieces.Contains(piece)))) {
        CheckPieceMovement(piece.Owner, piece.Position, GetMovement(piece, player), new List<Point>());
        if (ForcedMoves) {
          bool isHop = false;
          currentOptions.ForEach(delegate (MoveOption option) {
            if (option.HoppedPieces.Count > 0) {
              isHop = true;
            }
          });
          if (isHop) {
            currentOptions.RemoveAll(o => o.HoppedPieces.Count == 0);
          }
        }
        // TODO: do something in here that if ForcedMoves is on we need to remove simple moves if there are hopped options
        selectedPiece = piece;
        return currentOptions;
      } else {
        currentOptions = oldOptions;
        return null;
      }
    }

    public void StartNextTurn() {
      (bool over, PlayerType winner) = CheckGameOver();
      if (!over) {
        if (!Started) {
          currentPlayer = PlayerType.Player1;
          Started = true;
        } else if (currentPlayer == PlayerType.Player1) {
          currentPlayer = PlayerType.Player2;
        } else {
          currentPlayer = PlayerType.Player1;
        }
        if (ForcedMoves) {
          CheckForcedPieces(currentPlayer);
        }
        NextTurn?.Invoke(currentPlayer, forcedPieces);
      } else {
        GameOver?.Invoke(winner);
      }
    }

    private List<Point> GetMovement(GamePiece piece, PlayerType player) {
      List<Point> movement;
      if (piece.Type == PieceType.King) {
        movement = KingMovement;
      } else {
        movement = player == PlayerType.Player1 ? Player1Movement : Player2Movement;
      }
      return movement;
    }

    private void CheckForcedPieces(PlayerType player) {
      forcedPieces = new List<GamePiece>();
      List<GamePiece> playersPieces = (from piece in pieces where piece.Owner == player select piece).ToList();
      playersPieces.ForEach(delegate (GamePiece piece) {
        GetMovement(piece, player).ForEach(delegate (Point move) {
          Point moveSpot = new Point(piece.Position.X + move.X, piece.Position.Y + move.Y);
          GamePiece hopPiece = pieces.Find(p => p.Position.Equals(moveSpot));
          if (hopPiece != null && hopPiece.Owner != piece.Owner) {
            Point hopSpot = new Point(hopPiece.Position.X + move.X, hopPiece.Position.Y + move.Y);
            if (ValidMoveSpot(hopSpot)) {
              GamePiece landingPiece = pieces.Find(p => p.Position.Equals(hopSpot));
              if (landingPiece == null) {
                forcedPieces.Add(piece);
              }
            }
          }
        });
      });
    }

    private void CheckPieceMovement(PlayerType owner, Point position, List<Point> movement, List<Point> hoppedPieces) {
      int hoppedCount = hoppedPieces.Count;
      bool hasHop = false;
      movement.ForEach(delegate (Point move) {
        Point moveSpot = new Point(position.X + move.X, position.Y + move.Y);
        if (ValidMoveSpot(moveSpot)) {
          GamePiece hopPiece = pieces.Find(p => p.Position.Equals(moveSpot));
          if (hopPiece != null && hopPiece.Owner != owner && !hoppedPieces.Contains(hopPiece.Position)) {
            Point hopSpot = new Point(hopPiece.Position.X + move.X, hopPiece.Position.Y + move.Y);
            if (ValidMoveSpot(hopSpot)) {
              GamePiece landingPiece = pieces.Find(p => p.Position.Equals(hopSpot));
              if (landingPiece == null) {
                hasHop = true;
                List<Point> newHoppedPieces = new List<Point>(hoppedPieces);
                newHoppedPieces.Add(hopPiece.Position);
                CheckPieceMovement(owner, hopSpot, movement, newHoppedPieces);
              }
            } else if (hasHop) {
              currentOptions.Add(new MoveOption(position, new List<Point>(hoppedPieces)));
            }
          } else if (hopPiece == null && hoppedCount == 0) {
            currentOptions.Add(new MoveOption(moveSpot, new List<Point>(hoppedPieces)));
          }
        }
      });
      if (!hasHop && hoppedCount > 0) {
        currentOptions.Add(new MoveOption(position, new List<Point>(hoppedPieces)));
      }
    }

    private bool ValidMoveSpot(Point spot) {
      if (spot.X > 7 || spot.X < 0 || spot.Y > 7 || spot.Y < 0) {
        return false;
      }
      return true;
    }

    private (bool, PlayerType) CheckGameOver() {
      List<GamePiece> player1Pieces = (from piece in pieces where piece.Owner == PlayerType.Player1 select piece).ToList();
      List<GamePiece> player2Pieces = (from piece in pieces where piece.Owner == PlayerType.Player2 select piece).ToList();
      if (player1Pieces.Count == 0) {
        return (true, PlayerType.Player2);
      } else if (player2Pieces.Count == 0) {
        return (true, PlayerType.Player1);
      } else {
        return (false, PlayerType.Player1);
      }
    }
  }
}
