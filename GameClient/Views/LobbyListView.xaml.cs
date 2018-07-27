using System;
using System.Collections.ObjectModel;
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

using GameClient.Views;

namespace GameClient.Views {
  public partial class LobbyListView : UserControl {
    public LobbyListView() {
      InitializeComponent();

      MiddleManAPI.RequestLobbyList();
      MiddleManAPI.NewLobbyList += (List<LobbyListItem> list) => {
        Dispatcher?.Invoke(() => LobbyListDataGrid.ItemsSource = list);
      };

      LobbyListDataGrid.SizeChanged += LobbyListDataGrid_SizeChanged;
    }

    private void LobbyListDataGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
      foreach (var column in LobbyListDataGrid.Columns) {
        column.MinWidth = column.ActualWidth;
        column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
      }
    }

    private void Row_DoubleClick(object sender, MouseButtonEventArgs e) {
      DataGridRow row = sender as DataGridRow;
      LobbyListItem item = row.Item as LobbyListItem;
      if (int.TryParse(item.ID, out int id)) {
        Dispatcher?.Invoke(() => ViewController.SetView(new LoadingLobbyView(LobbyType.User, id)));
      }
    }

    private void HostButton_Click(object sender, RoutedEventArgs e) {
      Dispatcher?.Invoke(() => ViewController.SetView(new LoadingLobbyView(LobbyType.Host)));
    }
    
    private void RefreshButton_Click(object sender, RoutedEventArgs e) {
      MiddleManAPI.RequestLobbyList();
    }
  }
}
