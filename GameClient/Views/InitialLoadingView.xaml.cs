using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GameClient.Views;

namespace GameClient.Views {
  public partial class InitialLoadingView : UserControl {
    public InitialLoadingView() {
      InitializeComponent();

      new Thread(ConnectToMiddleMan).Start();
    }

    public void ConnectToMiddleMan() {
      int attempts = 0;
      while (!MiddleManAPI.Connect() && attempts++ < 30) {
        Thread.Sleep(200);
      }
      MiddleManAPI.ConnectedSuccess += () => Dispatcher?.Invoke(() => ViewController.SetView(new LobbyListView()));
    }
  }
}
