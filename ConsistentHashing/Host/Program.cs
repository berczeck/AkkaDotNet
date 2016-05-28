using System;
using Akka.Actor;
using Akka.Routing;
using Core;

namespace Host
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var system = ActorSystem.Create("ConsistentHashingSystem"))
            {
                var router = system.ActorOf(Props.Create<CustomerActor>().WithRouter(new ConsistentHashingPool(5).WithHashMapping(o =>
{
    if (o is IHasCustomKey)
        return ((IHasCustomKey)o).Identifier;

    return null;
})));
                Console.WriteLine("PersistenceActormSystem started...");
                Console.ReadKey();
            }
        }
    }
}
