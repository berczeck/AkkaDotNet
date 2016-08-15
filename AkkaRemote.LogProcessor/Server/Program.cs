using Akka.Actor;
using Akka.IO;
using Akka.Routing;
using Core;
using static System.Console;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("LogProcessorSystem"))
            {
                ReadLine();
                system.WhenTerminated.Wait();
            }
        }
    }
}
