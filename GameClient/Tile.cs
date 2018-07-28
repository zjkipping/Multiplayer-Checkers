using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

namespace GameClient {
  public class Tile: BoardObject {
    public Tile(Point position, double height = 100, double width = 100) {
      Position = position;
      Shape = new Rectangle {
        Width = width,
        Height = height,
        Fill = new SolidColorBrush {
          Color = (position.X + position.Y) % 2 == 0 ? Color.FromRgb(255, 0, 0) : Color.FromRgb(45, 45, 45)
        },
        StrokeThickness = 2
      };
    }
  }
}
