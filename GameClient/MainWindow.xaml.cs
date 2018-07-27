using System.Windows;
using GameClient.Views;

namespace GameClient {
  public partial class MainWindow: Window {
    public MainWindow() {
      InitializeComponent();
      ViewController.ViewChanged += SetRenderView;
      Dispatcher?.Invoke(() => ViewController.SetView(new InitialLoadingView()));
    }

    private void SetRenderView(ViewChangeEventArgs args) {
      ViewRenderer.Content = args.view;
    }
  }
}
