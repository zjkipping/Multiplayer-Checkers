using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiddleManServer {
  class Program {
    static void Main(string[] args) {
      if (Server.Start()) {
        Console.WriteLine("Server Successfully Started On Port: " + Server.Port);
      } else {
        Console.WriteLine("Failed To Start Server, Try Again Later...");
      }
    }
  }
}
