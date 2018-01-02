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


        public Millionaire(String name)
        {
            tcp = new TcpClient();
            millDic = new Dictionary<string, millClass>();
            this.name = name;
            udp = new UdpClient(5696);

        }
        public void Looking()
        {
            while (true)
            {
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
                    String check = Encoding.UTF8.GetString(bytes);
                    Console.WriteLine(getData(check));

                }
                if (netStream.CanWrite)
                {
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(name);
                    netStream.Write(sendBytes, 0, sendBytes.Length);
                    

                }
                update(netStream);
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
            Thread t =new Thread(() => listen(netStream));
            t.Start();
            String a = Console.ReadLine();
            while (a[0] != '\r')
            {
                if (netStream.CanWrite)
                {
                    Byte[] sendBytes = Encoding.UTF8.GetBytes(a);

                    netStream.Write(sendBytes, 0, sendBytes.Length);
                }
                a = Console.ReadLine();
            
            }
            t.Abort();
        }
        public void listen(NetworkStream nt)
        {
            while (true)
            {
                if (nt.CanRead)
                {
                    byte[] bytes = new byte[tcp.ReceiveBufferSize];
                    nt.Read(bytes, 0, (int)tcp.ReceiveBufferSize);
                    String check = Encoding.UTF8.GetString(bytes);
                    Console.WriteLine(getData(check));
                }
            }
        }



    }
}
