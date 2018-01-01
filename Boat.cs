using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Millionaires_Problem
{
    class Boat
    {

        private TcpListener tcpListener;
        private String name;
        private static Dictionary<String, millClass> millDic= new Dictionary<string, millClass>();
        private static millClass richest;
        private static bool stop = true;
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        public Boat(String name)
        {
            this.name = name;
            IPAddress ip = IPAddress.Parse(GetLocalIPAddress());
            this.tcpListener = new TcpListener(0);
        }
        public static void stoptheBoat() {
            String a = Console.ReadLine();
            if(a[0]=='\r')
                stop = false;
            
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

            while (stop)
            {

                Socket socket = tcpListener.AcceptSocket();
                Thread startListening = new Thread(() => handleClient(socket));
                startListening.Start();
                
            }
            tcpListener.Stop();
        }
        public void handleClient(Socket socket)
        {
            byte[] msg = Encoding.ASCII.GetBytes("Wellcome to the " + name + "! What is your name?");
            socket.Send(msg);
            byte[] answer = new byte[256];
            socket.Receive(answer);
            String millName = Encoding.UTF8.GetString(answer);
            millClass temp = new millClass(socket, millName, 0);
            lock (millDic) {
                millDic.Add(millName, temp);
            }
            checkRich(temp);
            sendToAll(temp);
            while (stop)
            {
                byte[] conversation =new byte[256];
                socket.Receive(conversation);
                
            
                if (Encoding.UTF8.GetString(conversation)[0]=='\r')
                {
                    break;
                }
                int money=Int32.Parse(Encoding.UTF8.GetString(answer));
                temp.Money = money;
                checkRich(temp);
                sendToAll2(temp);

            }
            lock (millDic)
            {
               millDic.Remove(millName);
            }
            socket.Close();
            
            
            
        }
        public static void checkRich(millClass temp)
        {
            
                if (richest == null)
                {
                    richest = temp;
                }
                else
                {
                    lock (richest)
                    {
                        if (richest.Money < temp.Money)
                            richest = temp;
                    }
                }
            
        }

        public void sendToAll2(millClass newMill)
        {
            string message = newMill.Name + " has updated his/her income.The richest person on the boat right now is " + richest.Name;
            foreach (millClass mill in millDic.Values)
            {
                mill.socket.Send(Encoding.ASCII.GetBytes(message));
            }
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
                IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 5696);

                byte[] bytes = BitConverter.GetBytes(Int16.Parse(((IPEndPoint)tcpListener.LocalEndpoint).Port.ToString()));
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);

                byte[] bytesReturn = Encoding.ASCII.GetBytes("IntroToNets" + MakeName(name));
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
