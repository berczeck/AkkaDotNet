using System;
using Akka.Actor;
using Core;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("LocalActorSystem"))
            {
                var customerContext =
                    system.ActorSelection("akka.tcp://PersistenceActorSystem@127.0.0.1:8092/user/CustomerBoundedContext")
                .ResolveOne(TimeSpan.FromSeconds(3))
                .Result;

                //customerContext.Tell(new ComandoAgregarTarjeta(new Tarjeta("01212", "85214"), "Customer1"));
                //customerContext.Tell(new ComandoAgregarTarjeta(new Tarjeta("01232", "852876"), "Customer2"));

                customerContext.Tell(new ComandoRealizarTransaccion(new Transaccion("8547854", 542), "Customer1"));
                //customerContext.Tell(new ComandoRealizarTransaccion(new Transaccion("2325412", 987), "Customer2"));

                Console.WriteLine("LocalActorSystem started...");
                Console.ReadKey();
            }
        }
    }

    class LocalActorSystem : ReceiveActor
    {
        public LocalActorSystem()
        {
            Receive<string>(mensaje =>
            {
                
            });
        }
    }
}
