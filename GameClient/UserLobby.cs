using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

using GameClient.Views;

namespace GameClient {
  class UserLobby: BaseLobby {
    public UserLobby(Socket connection) : base(LobbyType.User) {
      NewResponse += HandleResponse;

      this.connection = connection;
      ListenThread = new Thread(Listen);
      ListeningToConnection = true;
      ListenThread.Start();
    }

    public void SendChatMessage(string message) {
      SendMessage("MESSAGE|" + message + "-" + User.Name);
    }

    private void HandleResponse(string type, string parameters) {
      switch (type) {
        case "MESSAGE":
          string[] sections = parameters.Split('-');
          GotNewMessage(sections[0], sections[1]);
          break;
        case "STARTED":
          Application.Current.Dispatcher?.Invoke(() => ViewController.SetView(new GameView(this)));
          break;
      }
    }
  }
}
