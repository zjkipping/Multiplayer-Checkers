using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiddleManServer {
  public enum ClientType {
    User,
    Host
  }

  public enum LobbyStatus {
    InLobby,
    InGame
  }

  public delegate void ClientDisconnectEventHandler(Client sender, ClientDisconnectEventArgs e);

  public class ClientDisconnectEventArgs : EventArgs {
    public ClientDisconnectReason reason;
    public ClientDisconnectEventArgs(ClientDisconnectReason value) {
      reason = value;
    }
  }

  public enum ClientDisconnectReason {
    Clean,
    NoPingResponse,
    SocketTimeout
  }

  public delegate void LobbyCloseEventHandler(Lobby sender, LobbyCloseEventArgs e);

  public class LobbyCloseEventArgs : EventArgs {
    public LobbyCloseReason reason;
    public LobbyCloseEventArgs(LobbyCloseReason value) {
      reason = value;
    }
  }

  public enum LobbyCloseReason {
    Clean,
    HostDisconnect
  }
}
