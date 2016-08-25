using System;
using Akka.Actor;
using Core;

namespace AkkaPopulation
{
    class Program
    {
        static void Main(string[] args)
        {
            var actorSystem = ActorSystem.Create("AkkaPopulation");
            var bulkCoordinator = actorSystem.ActorOf<BulkCoordinator>("BulkCoordinator");
            var batchSize = 1000;
            var iteration = 0;
            
            for (int i = 0; i < 100000; i++)
            {
                var claim = GetClaim(i);
                
                if (i == iteration * batchSize)
                {
                    iteration++;
                    bulkCoordinator.Tell(new Start());
                    bulkCoordinator.Tell(claim);
                }
                else if (i == (iteration * batchSize) - 1)
                {
                    bulkCoordinator.Tell(claim);
                    bulkCoordinator.Tell(new Process());
                } else if (i < iteration * batchSize)
                {
                    bulkCoordinator.Tell(claim);
                }
            }
            Console.ReadLine();

        }

        static Claim GetClaim(int i)
        {
            System.Threading.Thread.Sleep(1);
            return new Claim
            {
                Id = i,
                CaseName = $"Case name {i}",
                ScheduleNumber = $"Schedule {i}",
                Number = (i + DateTime.Now.Second).ToString(),
                ProjectId = i
            };
        }

    }

    class StartProcess { }

    class LocalActor : ReceiveActor
    {
        private readonly IActorRef _coordinator;
        public LocalActor(IActorRef coordinator)
        {
            _coordinator = coordinator;

            Receive<string>(x => Console.WriteLine(x));
            
        }
    }
}
