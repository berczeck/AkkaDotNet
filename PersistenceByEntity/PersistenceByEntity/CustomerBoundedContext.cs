using System;
using System.Collections.Generic;
using Akka.Actor;
using Core;

namespace PersistenceByEntity
{
    public class CustomerBoundedContext : ReceiveActor
    {
        private Dictionary<string, IActorRef> Customers = new Dictionary<string, IActorRef>();

        public CustomerBoundedContext()
        {
            Receive<ComandoAgregarTarjeta>(mensaje =>
            {
                EnviarMensaje(mensaje.CustomerId, mensaje);
            });

            Receive<ComandoRealizarTransaccion>(mensaje =>
            {
                EnviarMensaje(mensaje.CustomerId, mensaje);
            });
        }

        private void EnviarMensaje<T>(string id, T mensaje)
        {
            if (!Customers.ContainsKey(id))
            {
                var actor =
                    Context.ActorOf(Props.Create(() =>
                    new CustomerAgregate(id)), "CustomerAgregate" + id);
                
                Customers.Add(id, actor);
            }
            Console.WriteLine("CustomerAgregate" + id);

            Customers[id].Forward(mensaje);
        }
    }
}
