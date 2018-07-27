using System;
using System.Windows;
using System.Windows.Controls;

namespace GameClient {
  public delegate void ViewChangeHandler(ViewChangeEventArgs args);

  public class ViewChangeEventArgs: EventArgs {
    public UserControl view;
    public ViewChangeEventArgs(UserControl value) {
      view = value;
    }
  }

  public static class ViewController {
    public static event ViewChangeHandler ViewChanged;

    public static void SetView(UserControl view) {
      if (view != null) {
        Application.Current.Dispatcher.Invoke(delegate {
          ViewChanged?.Invoke(new ViewChangeEventArgs(view));
        });
      }
    }
  }
}
