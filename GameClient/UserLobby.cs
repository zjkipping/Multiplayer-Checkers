using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace GameClient {
  class UserLobby: BaseLobby {
    public UserLobby(Socket connection) : base(LobbyType.User) {

    }
  }
}
