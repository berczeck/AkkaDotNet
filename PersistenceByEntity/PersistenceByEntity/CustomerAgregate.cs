using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Persistence;
using Core;

namespace PersistenceByEntity
{
    //Source: https://github.com/Horusiath/AkkaCQRS

    public class EstadoCuenta : ReceivePersistentActor
    {
        private List<Transaccion> Transacciones = new List<Transaccion>();
        private readonly string customerId;
        public override string PersistenceId { get; }

        public EstadoCuenta(string customerId)
        {
            this.customerId = customerId;
            PersistenceId = $"{Context.Parent.Path.Name}-{Self.Path.Name}:{customerId}";

            Recover((Action<TransaccionRegistrada>)Apply);

            Command<ComandoRealizarTransaccion>(mensaje =>
            {
                Persist(new TransaccionRegistrada(mensaje.Transaccion), evento =>
                {
                    Transacciones.Add(evento.Transaccion);
                    Console.WriteLine($"EVT EstadoCuenta Cliente: {customerId} - {evento}");

                    if (Transacciones.Count % 3 == 0)
                    {
                        SaveSnapshot(new Transaccion(evento.Transaccion.NumeroCuenta, Transacciones.Sum(x => x.Monto)));
                    }
                });
            });

            Recover<SnapshotOffer>(offer =>
            {
                var transaccion = offer.Snapshot as Transaccion;
                if (transaccion != null)
                    Transacciones.Add(transaccion);
            });

            Command<SaveSnapshotSuccess>(exito => DeleteMessages(exito.Metadata.SequenceNr, false));
        }

        public void Apply(TransaccionRegistrada evento)
        {
            Transacciones.Add(evento.Transaccion);
            Console.WriteLine($"DB TransaccionRegistrada Cliente: {customerId} - {evento}");
        }

    }

    public class CustomerAgregate : ReceivePersistentActor
    {
        private readonly string id;
        public override string PersistenceId { get; }
        private List<Tarjeta> Tarjetas = new List<Tarjeta>();
        //private List<Transaccion> Transacciones = new List<Transaccion>();
        private IActorRef EstadoCuenta;

        public CustomerAgregate(string customerId)
        {
            id = customerId;
            PersistenceId = $"{Context.Parent.Path.Name}-{Self.Path.Name}:{customerId}";

            Recover((Action<TarjetaAgregada>)Apply);
            //Recover((Action<TransaccionRegistrada>)Apply);
            
            Command<ComandoAgregarTarjeta>(mensaje =>
            {
                // TODO more tings
                Persist(new TarjetaAgregada(mensaje.Tarjeta), evento =>
                {
                    Tarjetas.Add(evento.Tarjeta);
                    Console.WriteLine($"EVT TarjetaAgregada Cliente: {id} - {evento}");

                });
            });
            Command<ComandoRealizarTransaccion>(mensaje =>
            {
                // TODO more tings
                //Persist(new TransaccionRegistrada(mensaje.Transaccion), evento =>
                //{
                //    Transacciones.Add(evento.Transaccion);
                //    Console.WriteLine($"EVT TransaccionRegistrada Cliente: {id} - {evento}");
                //});
                Console.WriteLine($"EVT TransaccionRegistrada Cliente: {customerId} - {mensaje}");
                EstadoCuenta.Tell(mensaje);
            });
            
        }
        
        public void Apply(TarjetaAgregada evento)
        {
            Tarjetas.Add(evento.Tarjeta);
            Console.WriteLine($"DB TarjetaAgregada Cliente: {id} - {evento}");
        }

        //public void Apply(TransaccionRegistrada evento)
        //{
        //    Transacciones.Add(evento.Transaccion);
        //    Console.WriteLine($"DB TransaccionRegistrada Cliente: {id} - {evento}");
        //}

        protected override void PreStart()
        {
            EstadoCuenta = Context.ActorOf(Props.Create(() => new EstadoCuenta(id)));
            base.PreStart();
        }
    }
}
