using System.Windows;
using System.Windows.Shapes;

namespace GameClient {
  public class BoardObject {
    public Shape Shape { get => shape; set => shape = value; }
    public Point Position { get => position; set => position = value; }

    private Shape shape;
    private Point position;
  }
}
