using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Millionaires_Problem
{
    class Boat
    {
        private Boolean stop;
        private TcpListener tcpListener;
        private String name;

        public Boat(String name)
        {
            this.name = MakeName(name);
            this.tcpListener = new TcpListener(IPAddress.Any, 0);
            stop = true;
        }

        private string MakeName(string name)
        {
            if (name.Length < 32)
            {
                String temp = name + "%";
                while (temp.Length < 32)
                {
                    temp += " ";
                }
                return temp;
            }
            else if (name.Length > 32)
            {
                return name.Substring(0, 32);
            }
            return name;
        }

        public void startListen()
        {
            tcpListener.Start();
            Console.WriteLine(((IPEndPoint)tcpListener.LocalEndpoint).Port);
            while (stop)
            {

                Socket socket = tcpListener.AcceptSocket();
            }
            tcpListener.Stop();
        }
        public void Broadcast()
        {
            while (stop)
            {
                UdpClient client = new UdpClient();
                IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 5656);

                byte[] bytes = BitConverter.GetBytes(Int16.Parse(((IPEndPoint)tcpListener.LocalEndpoint).Port.ToString()));
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);

                byte[] bytesReturn = Encoding.ASCII.GetBytes("IntroToNets" + name);
                byte[] res = new byte[bytes.Length + bytesReturn.Length];
                for(int i = 0; i < bytesReturn.Length; i++)
                {
                    res[i] = bytesReturn[i];
                }
                int k = 0;
                for(int j = bytesReturn.Length; j < res.Length; j++)
                {
                    res[j] = bytes[k];
                    k++;
                }
                client.Send(res, res.Length, ip);
                client.Close();
                Thread.Sleep(60000);
            }
        }
    }
}
