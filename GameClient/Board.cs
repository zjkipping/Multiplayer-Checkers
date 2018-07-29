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

namespace GameClient {
  public class Board {
    private Canvas canvas = null;
    private List<Tile> Tiles = new List<Tile>();
    private List<Piece> Pieces = new List<Piece>();
    private LobbyType lobbyType;
    private SolidColorBrush WhiteBrush = new SolidColorBrush { Color = Color.FromRgb(255, 255, 255) };
    private SolidColorBrush BlueBrush = new SolidColorBrush { Color = Color.FromRgb(0, 0, 255) };
    private SolidColorBrush GreenBrush = new SolidColorBrush { Color = Color.FromRgb(0, 255, 0) };
    private SolidColorBrush PinkBrush = new SolidColorBrush { Color = Color.FromRgb(255, 0, 255) };
    private SolidColorBrush YellowBrush = new SolidColorBrush { Color = Color.FromRgb(255, 255, 0) };

    public delegate void TileClickEventHandler(Point position);
    public event TileClickEventHandler TileClick;

    public delegate void PieceClickEventHandler(Point position);
    public event PieceClickEventHandler PieceClick;

    public delegate void TileHoverChangeEventHandler(Point spot, bool hovering);
    public event TileHoverChangeEventHandler TileHoverChange;

    public Board(Canvas canvas, LobbyType lobbyType) {
      double length = canvas.ActualHeight > canvas.ActualWidth ? canvas.ActualWidth : canvas.ActualHeight;

      this.lobbyType = lobbyType;
      this.canvas = canvas;

      // creating the tile objects
      for (int x = 0; x < 8; x++) {
        for (int y = 0; y < 8; y++) {
          Tile new_tile = new Tile(new Point(x, y));
          new_tile.Shape.MouseDown += Tile_MouseDown;
          new_tile.Shape.IsMouseDirectlyOverChanged += Tile_IsMouseDirectlyOverChanged;
          Tiles.Add(new_tile);
        }
      }

      UpdateTileBounds(length);

      // adding tiles to the canvas board
      Tiles.ForEach(delegate (Tile tile) {
        Canvas.SetZIndex(tile.Shape, 0);
        canvas.Children.Add(tile.Shape);
      });

      // creating the piece object for Player2 (always the user)
      bool owner = (lobbyType == LobbyType.User);
      for (int y = 0; y < 3; y++) {
        for (int x = 0; x < 8; x++) {
          if (((x + 1) + y) % 2 == 0) {
            Piece new_piece = new Piece(new Point(x, y), PlayerType.Player2);
            new_piece.Shape.MouseDown += Piece_MouseDown;
            Pieces.Add(new_piece);
          }
        }
      }

      // creating the piece objects for Player1 (always the host)
      for (int y = 7; y > 4; y--) {
        for (int x = 7; x >= 0; x--) {
          if (((x + 1) + y) % 2 == 0) {
            Piece new_piece = new Piece(new Point(x, y), PlayerType.Player1);
            new_piece.Shape.MouseDown += Piece_MouseDown;
            Pieces.Add(new_piece);
          }
        }
      }

      UpdatePieceBounds(length);

      // placing on the screen
      Pieces.ForEach(delegate (Piece piece) {
        Canvas.SetZIndex(piece.Shape, 1);
        canvas.Children.Add(piece.Shape);
      });

      canvas.SizeChanged += Canvas_SizeChanged;
    }

    public void HighlightForcedPiece(Point position) {
      Piece piece = Pieces.Find(p => p.Position.Equals(position));
      if (piece != null) {
        piece.Shape.Stroke = BlueBrush;
      }
    }

    public void ResetPieceHighlight(Point position) {
      Piece piece = Pieces.Find(p => p.Position.Equals(position));
      if (piece != null) {
        piece.Shape.Stroke = WhiteBrush;
      }
    }

    public void HighlightSelectedPiece(Point position) {
      Piece piece = Pieces.Find(p => p.Position.Equals(position));
      if (piece != null) {
        piece.Shape.Stroke = YellowBrush;
      }
    }

    public void ResetTileHighlight(Point spot) {
      Tile tile = Tiles.Find(t => t.Position.Equals(spot));
      if (tile != null) {
        tile.Shape.Stroke = null;
      }
    }

    public void HighlightTile(Point spot) {
      Tile tile = Tiles.Find(t => t.Position.Equals(spot));
      if (tile != null) {
        tile.Shape.Stroke = GreenBrush;
      }
    }

    public void HighlightHopPiece(Point position) {
      Piece piece = Pieces.Find(p => p.Position.Equals(position));
      if (piece != null) {
        piece.Shape.Stroke = PinkBrush;
      }
    }

    public void MovePiece(Point current, Point landing) {
      Piece piece = Pieces.Find(p => p.Position.Equals(current));
      if (piece != null) {
        piece.Position = landing;
        UpdatePieceBounds(canvas.ActualHeight > canvas.ActualWidth ? canvas.ActualWidth : canvas.ActualHeight);
      }
    }

    public void KingPiece(Point position) {
      Piece piece = Pieces.Find(p => p.Position.Equals(position));
      if (piece != null) {
        piece.Type = PieceType.King;
        piece.ChangeToKing(canvas);
        piece.Shape.MouseDown += Piece_MouseDown;
        UpdatePieceBounds(canvas.ActualHeight > canvas.ActualWidth ? canvas.ActualWidth : canvas.ActualHeight);
      }
    }

    public void RemovePiece(Point position) {
      Piece piece = Pieces.Find(p => p.Position.Equals(position));
      if (piece != null) {
        canvas.Children.Remove(piece.Shape);
        Pieces.Remove(piece);
      }
    }

    private void Piece_MouseDown(object sender, MouseButtonEventArgs e) {
      Piece piece = Pieces.Find(p => p.Shape.Equals((Shape)sender));
      if (piece.Player == User.Type) {
        PieceClick?.Invoke(piece.Position);
      }
    }

    private void Tile_MouseDown(object sender, MouseButtonEventArgs e) {
      Tile tile = Tiles.Find(t => t.Shape.Equals((Shape)sender));
      if (tile != null) {
        TileClick?.Invoke(tile.Position);
      }
    }

    private void Tile_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e) {
      Tile tile = Tiles.Find(t => t.Shape.Equals((Shape)sender));
      if (tile != null) {
        TileHoverChange?.Invoke(tile.Position, tile.Shape.IsMouseDirectlyOver);
      }
    }

    private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e) {
      // TODO: force the board to be a square always (take the smallest bounds and use that for both...)
      double length = canvas.ActualHeight > canvas.ActualWidth ? canvas.ActualWidth : canvas.ActualHeight;
      UpdateTileBounds(length);
      UpdatePieceBounds(length);
    }

    private void UpdateTileBounds(double length) {
      double tileHeight = length / 8;
      double tileWidth = length / 8;

      Tiles.ForEach(delegate (Tile tile) {
        tile.Shape.Width = tileWidth;
        tile.Shape.Height = tileHeight;
        Canvas.SetTop(tile.Shape, tile.Position.Y * tile.Shape.Height);
        Canvas.SetLeft(tile.Shape, tile.Position.X * tile.Shape.Width);
      });
    }

    private void UpdatePieceBounds(double length) {
      double tileHeight = length / 8;
      double tileWidth = length / 8;

      Pieces.ForEach(delegate (Piece piece) {
        double padding = 0.15;
        if (piece.Type == PieceType.King) {
          padding = .25;
          piece.SetKingPoints(tileWidth * (1 - padding), tileHeight * (1 - padding));
        } else {
          piece.Shape.Width = tileWidth * (1 - padding);
          piece.Shape.Height = tileHeight * (1 - padding);
        }

        double paddingWidth = tileWidth * padding;
        double paddingHeight = tileHeight * padding;

        Canvas.SetTop(piece.Shape, (piece.Position.Y * tileHeight) + (paddingHeight / 2));
        Canvas.SetLeft(piece.Shape, (piece.Position.X * tileWidth) + (paddingWidth / 2));
      });
    }
  }
}
