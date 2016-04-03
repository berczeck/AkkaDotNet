using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Persistence;
using Core;

namespace PersistenceByEntity
{
    //Source: https://github.com/Horusiath/AkkaCQRS

    public class CustomerAgregate : ReceivePersistentActor
    {
        private readonly string id;
        public override string PersistenceId { get; }
        private List<Tarjeta> Tarjetas = new List<Tarjeta>();
        private List<Transaccion> Transacciones = new List<Transaccion>();

        public CustomerAgregate(string customerId)
        {
            id = customerId;
            PersistenceId = $"{Context.Parent.Path.Name}-{Self.Path.Name}:{customerId}";

            Recover((Action<TarjetaAgregada>)Apply);
            Recover((Action<TransaccionRegistrada>)Apply);

            Recover<SnapshotOffer>(offer =>
            {
                var tarjetas = offer.Snapshot as List<Tarjeta>;
                if (tarjetas != null)
                {
                    Tarjetas = tarjetas;
                    return;
                }

            });

            Command<ComandoAgregarTarjeta>(mensaje =>
            {
                // TODO more tings
                Persist(new TarjetaAgregada(mensaje.Tarjeta), Apply);
            });
            Command<ComandoRealizarTransaccion>(mensaje =>
            {
                // TODO more tings
                Persist(new TransaccionRegistrada(mensaje.Transaccion), Apply);
            });
        }
        
        public void Apply(TarjetaAgregada evento)
        {
            Tarjetas.Add(evento.Tarjeta);
            Console.WriteLine($"TarjetaAgregada Cliente: {id} - {evento}");
        }

        public void Apply(TransaccionRegistrada evento)
        {
            Transacciones.Add(evento.Transaccion);
            Console.WriteLine($"TransaccionRegistrada Cliente: {id} - {evento}");
        }
        
    }
}
