using Akka.Actor;
using Core;

namespace Console
{
    public class LocalActor : ReceiveActor
    {
        private readonly IActorRef _logCoordinator;
        public LocalActor(IActorRef logCoordinator)
        {
            _logCoordinator = logCoordinator;
            Receive<string>(x => Start(x));

            Receive<LogFolderProcessed>(
                x =>
                {
                    System.Console.WriteLine($"END Path:{x.Path} NroFiles:{x.NumberOfFiles} Time (seconds):{x.ExecutionTime.TotalSeconds}");
                    System.Console.WriteLine("Insert path:");
                });

            Receive<LogFileProcessed>(
                x => System.Console.WriteLine($"File processed: {x.Path}"));
        }

        private void Start(string folder)
        {
            _logCoordinator.Tell(new ProcessLogFolder(folder));
        }
    }
}
