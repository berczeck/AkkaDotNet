using Akka.Actor;
using Core;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("ClientLogProcessor"))
            {                
                var logCoordinatorActor = system.ActorOf(Props.Create<LogProcessorCoordinator>(), "LogProcessorCoordinator");
                var localActor = system.ActorOf(Props.Create(() => new LocalActor(logCoordinatorActor)), "LocalActor");

                System.Console.WriteLine("Insert path:");
                var command = System.Console.ReadLine();

                while(!string.IsNullOrWhiteSpace(command))
                {
                    if(command.ToUpper().Equals("END"))
                    {
                        break;
                    }
                    if (System.IO.Directory.Exists(command))
                    {
                        System.Console.WriteLine($"Processing folder {command}");
                        localActor.Tell(command);
                    }
                    else
                    {
                        System.Console.WriteLine($"The path {command} doesn't exist");
                    }

                    command = System.Console.ReadLine();
                }         

                System.Console.Read();
            }
        }
    }
}
