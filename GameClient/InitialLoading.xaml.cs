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
  public partial class InitialLoading: UserControl {
    public InitialLoading() {
      InitializeComponent();
    }

    private void ChangeScreens_Click(object sender, RoutedEventArgs e) {
      ViewController.SetView(View.MainMenu);
    }
  }
}
