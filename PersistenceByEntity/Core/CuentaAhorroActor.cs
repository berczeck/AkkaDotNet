using System.Collections.Generic;
using System.Linq;
using Akka.Persistence;

namespace Core
{
    public class CuentaAhorroActor : ReceivePersistentActor 
    {
        public override string PersistenceId { get; }

        public CuentaAhorroActor()
        {
            PersistenceId = Context.Parent.Path.Name + "-" + Self.Path.Name;

            Recover<Deposito>(deposito => AgregarDeposito(deposito));

            Recover<SnapshotOffer>(offer =>
            {
                var deposito = offer.Snapshot as Deposito;
                if (deposito != null)
                    Depositos.Add(deposito);
            });
            
            Command<SaveSnapshotSuccess>(exito => DeleteMessages(exito.Metadata.SequenceNr, false));

            Command<ComandoRealizarDeposito>(cmd => Persist(cmd.Deposito, handler =>  Handle(cmd.Deposito)));
            Command<RequestConsultarDeposito>(req => Handle(req));            
        }
        
        private List<Deposito> Depositos = new List<Deposito>();

        private void AgregarDeposito(Deposito deposito)
        {
            Depositos.Add(deposito);
        }

        private List<Deposito> ObtenerDepositoCliente(string cliente)
        {
            return Depositos.Where(x => x.Cliente.Equals(cliente)).ToList();
        }

        public void Handle(RequestConsultarDeposito message)
        {
            var listaDepositos = ObtenerDepositoCliente(message.Cliente);

            Sender.Tell(new ResponseConsultarDeposito(listaDepositos), Self);
        }

        public void Handle(Deposito deposito)
        {
            AgregarDeposito(deposito);
            var cliente = deposito.Cliente;
            var listaDepositos = ObtenerDepositoCliente(cliente);
            var cantidadDepositos = listaDepositos.Count();

            if (cantidadDepositos % 3 == 0)
            {
                SaveSnapshot(new Deposito(cliente, listaDepositos.Sum(x => x.Monto)));
            }
        }
    }

    public class ResponseConsultarDeposito
    {
        public List<Deposito> Depositos { get; }

        public ResponseConsultarDeposito(List<Deposito> depositos)
        {
            Depositos = depositos;
        }
    }

    public class RequestConsultarDeposito
    {
        public string Cliente { get; }

        public RequestConsultarDeposito(string cliente)
        {
            Cliente = cliente;
        }
    }

    public class ComandoRealizarDeposito
    {
        public Deposito Deposito { get; }

        public ComandoRealizarDeposito(Deposito deposito)
        {
            Deposito = deposito;
        }
    }

    public class Deposito
    {
        public string Cliente { get; set; }
        public decimal Monto { get; set; }

        public Deposito()
        {

        }

        public Deposito(string cliente, decimal monto)
        {
            Cliente = cliente;
            Monto = monto;
        }
    }
}
