using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace GameClient {
  class HostLobby: BaseLobby {
    public HostLobby(): base(LobbyType.Host) {

    }
  }
}
