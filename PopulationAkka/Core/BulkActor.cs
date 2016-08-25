using System.Collections.Generic;
using Akka.Actor;

namespace Core
{
    public class BulkActor : ReceiveActor
    {
        List<Claim> list = new List<Claim>();

        public BulkActor()
        {
            Receive<Claim>(x =>
            {
                System.Console.WriteLine($"{Self.Path} {x}");
                list.Add(x);
            });
            Receive<Process>(x => Bulk(list));
        }

        private void Bulk(List<Claim> list)
        {
            System.Threading.Thread.Sleep(2000);
            Self.Tell(PoisonPill.Instance);
        }
    }

    public class Claim
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Number { get; set; }
        public string ScheduleNumber { get; set; }
        public string CaseName { get; set; }
    }

    public class Process
    {
    }
}
