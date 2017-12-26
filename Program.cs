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
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, what is the name of the Boat?");
            String a = Console.ReadLine();
            Boat shani = new Boat(a);
            Thread startListening = new Thread(new ThreadStart(shani.startListen));
            startListening.Start();
            Console.WriteLine("Hello, what is your name, Millionaire ?");
            String b = Console.ReadLine();
            Millionaire alex = new Millionaire(b);
            Thread mill = new Thread(new ThreadStart(alex.Looking));
            mill.Start();
            Thread broadcast = new Thread(new ThreadStart(shani.Broadcast));
            broadcast.Start();

            Console.WriteLine("YALLA POTHIM PORTIM");
        }

 
    }
    


}





