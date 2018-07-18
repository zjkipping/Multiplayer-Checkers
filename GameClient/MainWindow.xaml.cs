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
using System.Windows.Shapes;

namespace GameClient {
  public partial class MainWindow: Window {
    public MainWindow() {
      InitializeComponent();
      ViewRenderer.Content = new InitialLoading();
      ViewController.SetView(View.InitialLoading);

      ViewController.ViewChanged += SetRenderView;
    }

    private void SetRenderView(ViewChangeEventArgs args) {
      ViewRenderer.Content = args.view;
    }
  }
}
