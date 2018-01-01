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
        private String name;
        private UdpClient udp;
        private String boatName="";
        private TcpClient tcp;
        Dictionary<String, millClass> millDic;


        public Millionaire(String name)
        {
            millDic = new Dictionary<string, millClass>();
            this.name = name;
            udp = new UdpClient(5656);

        }
        public void Looking()
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Console.WriteLine("[Looking for a new boat...]");
            byte[] data=udp.Receive(ref iPEndPoint);
            byte[] temp = new byte[32];
            byte[] portArr = new byte[2];

            Array.Copy(data, 11, temp, 0, 32);
            Array.Copy(data, data.Length - 2, portArr, 0, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(portArr);
            int port= BitConverter.ToInt16(portArr, 0);
            String boatNameTemp = Encoding.Default.GetString(temp);
            for(int i = 0; i < boatNameTemp.Length; i++)
            {
                if (boatNameTemp[i] == '%')
                {
                    boatName += boatNameTemp[i];
                }
            }
            Console.WriteLine("[Requesting to board "+ boatName+"...]");
            tcp.Connect(iPEndPoint.Address, port);
            Console.WriteLine("[I am now aboard "+boatName+"!]");


        }


    }
}
