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
    class Millionaire 
    {
        private String name;
        private UdpClient udp;
        private String boatName="";
        private TcpClient tcp;
        Dictionary<String, millClass> millDic;
        private bool stop = false;
        private static bool groupStop = false;
        private Thread[] threadPool = new Thread[2];

        public Millionaire(String name)
        {
            
            millDic = new Dictionary<string, millClass>();
            this.name = name;
           

        }
        public void Looking()
        {
            while (true)
            {
                IPEndPoint localpt = new IPEndPoint(IPAddress.Any, 4569);
                udp = new UdpClient();
                udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udp.Client.Bind(localpt);
                boatName = "";
                stop = false;
                tcp = new TcpClient();
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Console.WriteLine("[Looking for a new boat...]");
                byte[] data = udp.Receive(ref iPEndPoint);
                byte[] temp = new byte[32];
                byte[] portArr = new byte[2];

                Array.Copy(data, 11, temp, 0, 32);
                Array.Copy(data, data.Length - 2, portArr, 0, 2);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(portArr);
                int port = BitConverter.ToInt16(portArr, 0);
                String boatNameTemp = Encoding.Default.GetString(temp);
                for (int i = 0; i < boatNameTemp.Length; i++)
                {
                    if (boatNameTemp[i] != '%')
                    {
                        boatName += boatNameTemp[i];
                    }
                    else
                    {
                        break;
                    }
                }
                Console.WriteLine("[Requesting to board " + boatName + "...]");

                tcp.Connect(iPEndPoint.Address, port);
                Console.WriteLine("[I am now aboard " + boatName + "!]");
                NetworkStream netStream = tcp.GetStream();
                if (netStream.CanRead)
                {
                    byte[] bytes = new byte[tcp.ReceiveBufferSize];
                    netStream.Read(bytes, 0, (int)tcp.ReceiveBufferSize);
                    String check = Encoding.ASCII.GetString(bytes);
                    Console.WriteLine(getData(check));
                }
                if (netStream.CanWrite)
                {
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(name);
                    netStream.Write(sendBytes, 0, sendBytes.Length);


                }
                groupStop = false;
                Thread u = new Thread(() => update(netStream));
                threadPool[0] = u;
                Thread l = new Thread(() => listen(netStream));
                threadPool[1] = l;
                u.Start();
                l.Start();
                u.Join();
                /*while (true)
                {
                    if (stop||groupStop)
                    {
                        u.Abort();
                        l.Abort();
                        if (netStream.CanRead)
                        {
                            netStream.Close();
                        }
                        boatName="";
                        break;
                        

                    }
                }*/
            }
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
        public void update(NetworkStream netStream)
        {
            String a = Console.ReadLine();
            while(true){

                if (netStream.CanWrite)
                {
                    a = a + "\r\n";
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(a);
                    netStream.Write(sendBytes, 0, sendBytes.Length);

                }
                if (a[0] == '\r'&& a[1] == '\n')
                {
                    threadPool[1].Abort();
                    threadPool[0].Abort();
                }
                a = Console.ReadLine();
            }
        }
        public void listen(NetworkStream nt)
        {
            
            
                while (true)
                {
                    if (nt.CanRead)
                    {
                        byte[] bytes = new byte[tcp.ReceiveBufferSize];
                        nt.Read(bytes, 0, (int)tcp.ReceiveBufferSize);
                        String check = Encoding.ASCII.GetString(bytes);
                        if (check[0] == '\0')
                        {
                            threadPool[0].Abort();
                            threadPool[1].Abort();
                        }
                        Console.WriteLine(getData(check));
                    }
                }
            
            
              
            
        }



    }
}
