using System;
using Akka.Actor;
using Core;

namespace PersistenceByEntity
{
    public class CustomerBoundedContext : ReceiveActor
    {
        public CustomerBoundedContext()
        {
            Receive<ComandoAgregarTarjeta>(mensaje =>
            {
                //var actorTest = Context.ActorSelection("/CustomerAgregate" + mensaje.CustomerId);
                //actorTest.Tell(mensaje);

                var actor =  Context.ActorOf(Props.Create(() =>
                new CustomerAgregate(mensaje.CustomerId)), "CustomerAgregate"+ mensaje.CustomerId);
                Console.WriteLine("CustomerAgregate" + mensaje.CustomerId);
                actor.Forward(mensaje);
            });

            Receive<ComandoRealizarTransaccion>(mensaje =>
            {
                var actor = Context.ActorOf(Props.Create(() =>
               new CustomerAgregate(mensaje.CustomerId)), "CustomerAgregate" + mensaje.CustomerId);
                Console.WriteLine("CustomerAgregate" + mensaje.CustomerId);
                actor.Forward(mensaje);
            });
        }
    }
}
