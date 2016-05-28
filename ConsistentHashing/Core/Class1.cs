using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Persistence;

namespace Core
{
    public interface IHasCustomKey
    {
        string Identifier { get; }
    }
    public class CustomerActor : ReceivePersistentActor, IHasCustomKey
    {
        public string Identifier
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string PersistenceId { get; }

        public CustomerActor()
        {
            PersistenceId = $"{Context.Parent.Path.Name}-{Self.Path.Name}";
        }

    }
}
