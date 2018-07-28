using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

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
  }
}
