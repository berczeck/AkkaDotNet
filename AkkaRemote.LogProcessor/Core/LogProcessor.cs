using System;
using System.IO;
using Akka.Actor;

namespace Core
{
    public class LogProcessor : ReceiveActor
    {
        private int _numberOfLines;
        public LogProcessor()
        {
            Receive<ProcessFile>(x => Execute(x));
        }

        private void Execute(ProcessFile file)
        {
            foreach (var line in File.ReadLines(file.Path))
            {
                _numberOfLines++;
            }
            Console.WriteLine($"{file.Path} - Rows: {_numberOfLines} ThreadId: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            Self.Tell(PoisonPill.Instance);
        }
    }
}
