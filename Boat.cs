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
        private Dictionary<String, millClass> millDic;
        private millClass richest;
        public Boat(String name)
        {
            this.millDic = new Dictionary<string, millClass>();
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
                byte[] msg = Encoding.ASCII.GetBytes("Wellcome to the " + name + "! What is your name?");
                socket.Send(msg);
                byte[] answer = new byte[256];
                socket.Receive(answer);
                String millName = Encoding.UTF8.GetString(answer);

                millClass temp = new millClass(socket, millName, 0);
                millDic.Add(millName, temp));

                if (richest == null)
                {
                    richest = temp;
                }
                else
                {
                    if (richest.Money < temp.Money)
                        richest = temp;
                }


            }
            tcpListener.Stop();
        }
        public void sendToAll(millClass newMill) {
            string message = "A Millionaire named " + newMill.Name + " has joined the boat.The richest person on the boat right now is " + richest.Name;
            foreach(millClass mill in millDic.Values)
            {
                mill.socket.Send(Encoding.ASCII.GetBytes(message));
            }
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
