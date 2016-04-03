using System;
using Akka.Actor;
using Core;
using MongoDB.Bson.Serialization;

namespace PersistenceByEntity
{
    class Program
    {
        static void Main(string[] args)
        {
            BsonClassMap.RegisterClassMap<Core.Transaccion>();
            BsonClassMap.RegisterClassMap<Core.TransaccionRegistrada>();
            BsonClassMap.RegisterClassMap<Core.Tarjeta>();
            BsonClassMap.RegisterClassMap<Core.TarjetaAgregada>();

            using (var system = ActorSystem.Create("PersistenceActorSystem"))
            {
                var actor = system.ActorOf<CustomerBoundedContext>("CustomerBoundedContext");
                Console.WriteLine("PersistenceActormSystem started...");
                Console.WriteLine($"{actor.Path} {actor}");
                Console.ReadKey();
            }
        }
    }
}