using System;
using System.Collections.Generic;
using System.Linq;
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

            //Recover<SnapshotOffer>(offer =>
            //{
            //    var tarjetas = offer.Snapshot as List<Tarjeta>;
            //    if (tarjetas != null)
            //    {
            //        Tarjetas = tarjetas;
            //        return;
            //    }
            //     var transaccion = offer.Snapshot as Transaccion;
            //    if (transaccion != null)
            //    {
            //        Transacciones.Add(transaccion);
            //        return;
            //    }
            //});

            Command<ComandoAgregarTarjeta>(mensaje =>
            {
                // TODO more tings
                Persist(new TarjetaAgregada(mensaje.Tarjeta), evento =>
                {
                    Tarjetas.Add(evento.Tarjeta);
                    Console.WriteLine($"EVT TarjetaAgregada Cliente: {id} - {evento}");
                    //if (Tarjetas.Count % 3 == 0)
                    //{
                    //    SaveSnapshot(Tarjetas);
                    //}
                });
            });
            Command<ComandoRealizarTransaccion>(mensaje =>
            {
                // TODO more tings
                Persist(new TransaccionRegistrada(mensaje.Transaccion), evento =>
                {
                    Transacciones.Add(evento.Transaccion);
                    Console.WriteLine($"EVT TransaccionRegistrada Cliente: {id} - {evento}");
                    //if (Transacciones.Count > 3)
                    //{
                    //    var total = Transacciones.Sum(x => x.Monto);
                    //    SaveSnapshot(new Transaccion(evento.Transaccion.NumeroCuenta, total));
                    //}
                });
            });
            
            //Command<SaveSnapshotSuccess>(exito =>
            //{
            //      DeleteMessages(exito.Metadata.SequenceNr, true);
            //});
        }
        
        public void Apply(TarjetaAgregada evento)
        {
            Tarjetas.Add(evento.Tarjeta);
            Console.WriteLine($"DB TarjetaAgregada Cliente: {id} - {evento}");
        }

        public void Apply(TransaccionRegistrada evento)
        {
            Transacciones.Add(evento.Transaccion);
            Console.WriteLine($"DB TransaccionRegistrada Cliente: {id} - {evento}");
        }
        
    }
}
