using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
  public partial class LobbyView : UserControl {
    public BaseLobby lobby;
    public ObservableCollection<string> ChatMessages = new ObservableCollection<string>();

    public LobbyView(BaseLobby lobby) {
      InitializeComponent();
      ChatListBox.ItemsSource = ChatMessages;
      this.lobby = lobby;

      lobby.NewMessage += (string message, string user) => {
        Dispatcher?.Invoke(() => ChatMessages.Add((user != "" ? user + ": " : "") + message));
      };

      if (lobby.Type == LobbyType.Host) {
        (lobby as HostLobby).StartEnabled += () => Dispatcher?.Invoke(() => { StartButton.IsEnabled = true; StartButton.Visibility = Visibility.Visible; });
      }
    }

    private void ChatButton_Click(object sender, RoutedEventArgs e) {
      if (lobby.Type == LobbyType.User) {
        (lobby as UserLobby).SendChatMessage(MessageText.Text);
        MessageText.Text = "";
      } else if (lobby.Type == LobbyType.Host) {
        (lobby as HostLobby).SendChatMessage(MessageText.Text);
        MessageText.Text = "";
      }
    }

    private void StartButton_Click(object sender, RoutedEventArgs e) {
      if (lobby.Type == LobbyType.Host) {
        (lobby as HostLobby).StartGame();
      }
    }
  }
}
