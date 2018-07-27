using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

using GameClient.Views;

namespace GameClient {
  class HostLobby: BaseLobby {
    public delegate void StartEnabledEventHandler();
    public event StartEnabledEventHandler StartEnabled;
    public Game game = null;

    public HostLobby(): base(LobbyType.Host) {
      MiddleManAPI.UserConnected += (Socket connection) => {
        this.connection = connection;
        ListenThread = new Thread(Listen);
        ListeningToConnection = true;
        ListenThread.Start();
        GotNewMessage("---New User Connected---");
        StartEnabled.Invoke();
      };

      NewResponse += HandleResponse;
    }

    public void StartGame() {
      game = new Game();
      SendMessage("STARTED|");
      ViewController.SetView(new GameView(this));
    }

    public void SendChatMessage(string message) {
      GotNewMessage(message, User.Name);
      SendMessage("MESSAGE|" + message + "-" + User.Name);
    }

    private void HandleResponse(string type, string parameters) {
      switch(type) {
        case "MESSAGE":
          string[] parts = parameters.Split('-');
          string message = parts[0];
          string user = parts[1];
          GotNewMessage(message, user);
          SendMessage("MESSAGE|" + parameters);
          break;
      }
    }
  }
}
