using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Routing;
using Core;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("ClientLogProcessor"))
            {
                //var remoteLogCoordinator = system.ActorOf(Props.Create<LogProcessorCoordinator>().WithRouter(FromConfig.Instance), "LogProcessorCoordinator");
                var logCoordinatorActor = system.ActorOf(Props.Create<LogProcessorCoordinator>(), "LogProcessorCoordinator");
                var localActor = system.ActorOf(Props.Create(() => new LocalActor(logCoordinatorActor)), "LocalActor");
                localActor.Tell(@"C:\Temp\DataPopulation");

                System.Console.Read();
            }
        }
        
        public class LocalActor : ReceiveActor
        {
            private readonly IActorRef _logCoordinator;
            public LocalActor(IActorRef logCoordinator)
            {
                _logCoordinator = logCoordinator;
                Receive<string>(x => Start(x));
                Receive<LogFolderProcessed>(
                    x => System.Console.WriteLine($"END Path:{x.Path} NroFiles:{x.NumberOfFiles} Time (seconds):{x.ExecutionTime.TotalSeconds}"));
                Receive<LogFileProcessed>(
                    x => System.Console.WriteLine($"File processed: {x.Path}"));
            }

            private void Start(string folder)
            {
                _logCoordinator.Tell(new ProcessLogFolder(folder));
            }
        }
    }
}
