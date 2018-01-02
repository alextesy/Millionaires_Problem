using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
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
            this.tcpListener = new TcpListener(ip, Port());
        }

        private int Port()
        {
            bool PortAvailable = false;
            int port = 0;
            Random randPort = new Random();
            //loop till you find a port which is available
            while (!PortAvailable)
            {
                //get a random number 
                port = randPort.Next(1000, 9999);
                //check if the port is not in use
                PortAvailable = this.PortAvailable(port);
            }
            return port;
        }
        //function to check if the port sent is in use
        //returns false if the port is in use or else return true
        private bool PortAvailable(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            System.Collections.IEnumerator myEnum = tcpConnInfoArray.GetEnumerator();
            while (myEnum.MoveNext())
            {
                TcpConnectionInformation tcpi = (TcpConnectionInformation)myEnum.Current;

                if (tcpi.LocalEndPoint.Port == port)
                {
                    return false;
                }
            }

            return true;
        }
        public static void stoptheBoat() {
            while (true)
            {
                String a = Console.ReadLine();
                stop = false;
                foreach (millClass m in millDic.Values)
                {
                    m.socket.Close();
                    m.t.Abort();
                }
                millDic = new Dictionary<string, millClass>();
            }
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

       public string getData(string data)
        {
            int i = 0;
            for(i = 0; i < data.Length; i++)
            {
                if (data[i] == '\0')
                    break;
            }
            return data.Substring(0, i);
        }
        public void handleClient(Socket socket)
        {
            byte[] msg = Encoding.ASCII.GetBytes("Wellcome to the " + name + "! What is your name?");
            socket.Send(msg);
            byte[] answer = new byte[256];
            socket.Receive(answer);
            String millName = getData(Encoding.ASCII.GetString(answer));
            String ip = ((IPEndPoint)socket.RemoteEndPoint).Address + ":" + ((IPEndPoint)socket.RemoteEndPoint).Port;
            millClass temp = new millClass(socket, millName, 0, Thread.CurrentThread);
            lock (millDic) {
                millDic.Add(ip, temp);
            }
            checkRich();
            sendToAll(temp);
            try
            {
                while (stop)
                {
                    byte[] conversation = new byte[256];
                    socket.Receive(conversation);
                    if (!SocketConnected(socket))//receive enter
                    {
                        break;
                    }
                    if (Int32.TryParse(Encoding.ASCII.GetString(conversation), out int money))
                    {
                        temp.Money = money;
                        checkRich();
                        sendToAll2(temp);
                    }
                }
                lock (millDic)
                {
                    Thread tmp = millDic[ip].t;
                    millDic.Remove(ip);
                    checkRich();
                    tmp.Abort();
                }
                socket.Close();
            }
            catch(SocketException e)//boat throw all the millionaires
            {

            }
        }
        bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        public static void checkRich()
        {
            int max = -1;
            foreach(millClass m in millDic.Values)
            {
                if(m.Money > max)
                {
                    richest = m;
                    max = m.Money;
                }
            }
        }

        public void sendToAll2(millClass newMill)
        {
            string message = newMill.Name + " has updated his/her income.The richest person on the boat right now is " + richest.Name;
            lock (millDic)
            {
                foreach (millClass mill in millDic.Values)
                {
                    mill.socket.Send(Encoding.ASCII.GetBytes(message));
                }
            }
        }
        public void sendToAll(millClass newMill) {
            string message = "A Millionaire named " + newMill.Name + " has joined the boat.The richest person on the boat right now is " + richest.Name;
            lock (millDic)
            {
                foreach (millClass mill in millDic.Values)
                {
                    mill.socket.Send(Encoding.ASCII.GetBytes(message));
                }
            }
        }
        public void Broadcast()
        {
            while (stop)
            {
                UdpClient client = new UdpClient();
                IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 4569);

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
