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
using System.Net.Sockets;

namespace GameClient.Views {
  public partial class LoadingLobbyView : UserControl {
    public LoadingLobbyView(LobbyType type, int lobbyID = 0) {
      InitializeComponent();
      if (type == LobbyType.Host) {
        StatusText.Text = "Creating A Lobby...";
        MiddleManAPI.HostLobby();
        MiddleManAPI.LobbyCreated += () => Dispatcher?.Invoke(() => ViewController.SetView(new LobbyView(new HostLobby())));
      } else if (type == LobbyType.User) {
        if (lobbyID == 0) {
          Dispatcher?.Invoke(() => ViewController.SetView(new LobbyListView()));
        } else {
          StatusText.Text = "Connecting To Lobby #" + lobbyID + "...";
          MiddleManAPI.JoinLobby(lobbyID);
          MiddleManAPI.JoinLobbySuccess += (Socket connection) => Dispatcher?.Invoke(() => ViewController.SetView(new LobbyView(new UserLobby(connection))));
          MiddleManAPI.JoinLobbyFailure += () => Dispatcher?.Invoke(() => ViewController.SetView(new LobbyListView()));
        }
      }
    }
  }
}
