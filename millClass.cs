using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Millionaires_Problem
{
    class millClass
    {
        public Socket socket { get; set; }
        public string Name { get; set; }
        public int Money { get; set; }
        public millClass(Socket socket,string name,int money)
        {
            this.socket = socket;
            this.Name = name;
            this.Money = money;
        }
    }
}
