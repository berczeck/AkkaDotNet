using System;
using Akka.Actor;

namespace AkkaPopulation.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("AkkaPopulation"))
            {
                Console.ReadLine();
                system.WhenTerminated.Wait();
            }
        }
    }
}
