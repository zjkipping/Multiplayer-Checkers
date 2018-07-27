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
    BaseLobby Lobby = null;
    Board board = null;
    public GameView(BaseLobby lobby) {
      InitializeComponent();

      Lobby = lobby;

      Lobby.ChatMessages.ForEach(delegate (string message) {
        ChatBox.Items.Add(message);
      });
    }

    private void CanvasBoard_Loaded(object sender, RoutedEventArgs e) {
      board = new Board(CanvasBoard);
    }
  }
}
