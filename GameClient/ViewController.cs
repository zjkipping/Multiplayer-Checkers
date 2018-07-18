using System;
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
    public static View CurrentView;

    public static event ViewChangeHandler ViewChanged;

    public static void SetView(View view) {
      switch (view) {
        case View.InitialLoading:
          CurrentView = view;
          ViewChanged?.Invoke(new ViewChangeEventArgs(new InitialLoading()));
          break;
        case View.MainMenu:
          CurrentView = view;
          ViewChanged?.Invoke(new ViewChangeEventArgs(new MainMenu()));
          break;
      }
    }
  }
}
