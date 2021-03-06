﻿using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace MiddleManServer {
  public class Client {
    private Socket Connection;
    private Thread ListenThread = null;
    private Thread PingThread = null;
    private Thread PingTask = null;
    private DateTime LastPing;
    private bool PingingThreadRunning = false;
    private bool PingingClient = false;
    private bool Listening;
    private const double PingRate = 10000;
    private const int PingWait = 5000;

    public event ClientDisconnectEventHandler ClientDisconnected;
    public Lobby Lobby = null;
    public ClientType Type { get; private set; }
    public string Name { get; private set; }

    public EndPoint IP {
      get {
        return Connection.RemoteEndPoint;
      }
      private set { }
    }

    public bool Connected {
      get {
        return Connection.Connected;
      }
      private set { }
    }

    public Client(Socket connection) {
      Name = "Guest#" + (Server.GetClientCount() + 1).ToString();
      Connection = connection;
      Type = ClientType.User;
      ListenThread = new Thread(Listen);
      PingThread = new Thread(Pinging);

      Console.WriteLine("Client Connected:    " + Connection.RemoteEndPoint + "    -    " + Connection.LocalEndPoint);
    }

    public void SetName(string value) {
      Name = value;
    }

    public void SetType(ClientType value) {
      Type = value;
    }

    public void Start() {
      if (Connection != null) {
        SendMessage("CONNECTED|"+Name);
        Listening = true;
        LastPing = DateTime.Now;
        ListenThread.Start();
        //PingingThreadRunning = true;
        //PingThread.Start();
      }
    }

    public void SendMessage(string message) {
      try {
        Connection.Send(Encoding.ASCII.GetBytes(message + "\r\n"));
      } catch (SocketException) {
        Disconnect(ClientDisconnectReason.SocketTimeout);
      }
    }


    public void Close() {
      PingingThreadRunning = false;
      Listening = false;
      if (PingTask != null) {
        PingTask.Abort();
      }
      Connection.Close();
    }

    private void Listen() {
      while (Listening) {
        byte[] buffer = new byte[1024];
        try {
          string response = Encoding.ASCII.GetString(buffer, 0, Connection.Receive(buffer));
          if (response == "") {
            Disconnect(ClientDisconnectReason.SocketTimeout);
            break;
          }
          string[] sections = response.Split('|');
          string responseType = sections[0];

          if (responseType == "PONG") {
            HandlePongResponse();
          } else if (responseType == "DISCONNECT") {
            Disconnect(ClientDisconnectReason.Clean);
          } else if (responseType == "REQ_LOBBIES") {
            Server.GetLobbyList(this);
          } else if (Type == ClientType.Host) {
            switch (responseType) {
              case "CLOSE_LOBBY":
                Server.CloseLobby(this);
                break;
              case "UPDATE_STATUS":
                int code = int.Parse(sections[1]);
                Lobby.SetStatus(code);
                break;
              case "PLAYER_CONNECT_SUCCESS":
                Server.ConnectSuccess(this);
                break;
              case "PLAYER_CONNECT_FAIL":
                Server.ConnectFail(this);
                break;
            }
          } else if (Type == ClientType.User) {
            switch (responseType) {
              case "HOST":
                Console.WriteLine("Got Host Response!");
                Server.HostLobby(this, sections[1]);
                break;
              case "CONNECT":
                int id = int.Parse(sections[1]);
                Server.JoinLobby(this, id);
                break;
              case "LEAVE":
                Server.LeaveLobby(this);
                break;
            }
          }
        } catch {
          Disconnect(ClientDisconnectReason.SocketTimeout);
        }
      }
    }

    private void HandlePongResponse() {
      Console.WriteLine("CIENT PONGED {0}", IP);
      LastPing = DateTime.UtcNow;
      if (PingTask != null) {
        PingTask.Abort();
      }
      PingingClient = false;
    }

    private void Pinging() {
      while (PingingThreadRunning) {
        if (LastPing.AddMilliseconds(PingRate) < DateTime.Now && !PingingClient) {
          PingingClient = true;
          Console.WriteLine("PING CLIENT AT {0}", IP);
          PingThread = new Thread(() => {
            PingClient();
            Thread.Sleep(PingWait);
            if (PingingClient) {
              Disconnect(ClientDisconnectReason.NoPingResponse);
            }
          });
          PingThread.Start();
        }
      }
    }

    private void PingClient() {
      try {
        Connection.Send(Encoding.ASCII.GetBytes("PING|\r\n"));
      } catch (SocketException) {
        Disconnect(ClientDisconnectReason.SocketTimeout);
      }
    }

    private void Disconnect(ClientDisconnectReason reason) {
      Close();
      ClientDisconnected?.Invoke(this, new ClientDisconnectEventArgs(reason));
    }
  }
}
