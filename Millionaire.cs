using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Millionaires_Problem
{
    class Millionaire 
    {
        String name;
        UdpClient udp;
        String boatName;
        TcpClient tcp;
        public Millionaire(String name)
        {
            this.name = name;
            udp = new UdpClient(5656);

        }
        public void Looking()
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Console.WriteLine("[Looking for a new boat...]");
            byte[] data=udp.Receive(ref iPEndPoint);
            Console.WriteLine("[Requesting to board The Royal Princess…]");
            byte[] temp = new byte[32];
            byte[] portArr = new byte[2];

            Array.Copy(data, 11, temp, 0, 32);
            Array.Copy(data, data.Length - 2, portArr, 0, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(portArr);
            int port= BitConverter.ToInt16(portArr, 0);
            boatName = Encoding.Default.GetString(temp);
            tcp.Connect(iPEndPoint.Address, port);
            Console.WriteLine("[I am now aboard The Royal Princess!]");


        }


    }
}
