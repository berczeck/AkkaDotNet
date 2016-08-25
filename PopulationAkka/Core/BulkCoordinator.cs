using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Akka.Actor;

namespace Core
{
    public class BulkCoordinator : ReceiveActor
    {
        private IActorRef bulkActor;
        private IList<IActorRef> bulkActorList = new List<IActorRef>();
        private int _docmentNumber = 0;
        private Stopwatch stopwatch = new Stopwatch();
        public BulkCoordinator()
        {
            // Begin timing.
            stopwatch.Start();
            Receive<Start>(x => Start());
            Receive<Claim>(x => Send(x));
            Receive<Process>(x => Process(x));
            Receive<Terminated>(x => Terminated(x));
        }

        private void Terminated(Terminated terminated)
        {
            Console.WriteLine($"Stop: {terminated.ActorRef}");
            bulkActorList.Remove(terminated.ActorRef);
            if(!bulkActorList.Any())
            {
                Console.WriteLine($"Document Processed:{_docmentNumber}");
                stopwatch.Stop();
                Console.WriteLine("Time elapsed Query: {0}", stopwatch.Elapsed);
            }
        }

        private void Start()
        {
            var actorProps = Props.Create(() => new BulkActor()).WithDeploy(
                new Deploy(new RemoteScope(new Address("akka.tcp", "AkkaPopulation", "localhost",9595))));

            //var actor = Context.ActorOf<BulkActor>(Guid.NewGuid().ToString());
            var actor = Context.ActorOf(actorProps, Guid.NewGuid().ToString());

            bulkActorList.Add(actor);
            Context.Watch(actor);

            bulkActor = actor;
            Console.WriteLine($"Start: {actor}");
        }

        private void Send(Claim claim)
        {
            _docmentNumber++;
            bulkActor.Tell(claim);
        }

        private void Process(Process process)
        {
            bulkActor.Tell(process);
            Console.WriteLine($"Documents: {_docmentNumber}");
        }
    }

    public class Start {}
}
