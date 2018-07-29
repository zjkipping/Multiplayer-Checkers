using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace GameClient {
  public class Piece: BoardObject {
    public PieceType Type;
    public PlayerType Player;

    public Piece(Point position, PlayerType player, double height = 75, double width = 75) {
      Position = position;
      Player = player;
      Type = PieceType.Normal;

      Shape = new Ellipse {
        Width = height,
        Height = width,
        Fill = new SolidColorBrush {
          Color = player == PlayerType.Player1 ? Color.FromRgb(0, 0, 0) : Color.FromRgb(255, 0, 0)
        },
        Stroke = new SolidColorBrush {
          Color = Color.FromRgb(255, 255, 255)
        },
        StrokeThickness = 2
      };
    }

    public void ChangeToKing(Canvas canvas, double height = 75, double width = 75) {
      canvas.Children.Remove(Shape);

      // set shape as a triangle
      Shape = new Polygon {
        Points = { new Point(0, height), new Point(width / 2, 0), new Point(width, height) },
        Fill = new SolidColorBrush {
          Color = Player == PlayerType.Player1 ? Color.FromRgb(0, 0, 0) : Color.FromRgb(255, 0, 0)
        },
        Stroke = new SolidColorBrush {
          Color = Color.FromRgb(255, 255, 255)
        },
        StrokeThickness = 2
      };

      canvas.Children.Add(Shape);
    }

    public void SetKingPoints(double height, double width) {
      (Shape as Polygon).Points = new PointCollection(new Point [] { new Point(0, height), new Point(width / 2, 0), new Point(width, height) });
    }
  }
}
