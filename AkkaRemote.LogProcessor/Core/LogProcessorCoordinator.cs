using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka.Actor;
using Akka.Util.Internal;

namespace Core
{
    public class LogProcessorCoordinator : ReceiveActor
    {
        private readonly IDictionary<ActorPath, string> _children = new Dictionary<ActorPath, string>();
        private IActorRef _caller;
        private ProcessLogFolder _logFolder;
        private int _numberOfFiles;
        private DateTime _processStartAt;

        public LogProcessorCoordinator()
        {
            Receive<ProcessLogFolder>(x => StartProcess(x));
            Receive<Terminated>(x => FileProcessed(x));
        }

        private void FileProcessed(Terminated message)
        {
            var file = _children[message.ActorRef.Path];
            _caller.Tell(new LogFileProcessed(file));
            _children.Remove(message.ActorRef.Path);
            if (!_children.Any())
            {
                _caller.Tell(new LogFolderProcessed(_logFolder.Path, _numberOfFiles,
                    DateTime.Now.Subtract(_processStartAt)));
            }
        }

        private void StartProcess(ProcessLogFolder message)
        {
            LogStartProcess();
            SetInitValues(message);

            Directory.EnumerateFiles(message.Path).ForEach(file =>
            {
                _numberOfFiles++;
                var child = Context.ActorOf<LogProcessor>($"{nameof(LogProcessor)}{_numberOfFiles}");
                WatchChild(child, file);
                child.Tell(new ProcessFile(file));
            });
        }

        private void SetInitValues(ProcessLogFolder message)
        {
            _caller = Context.Sender;
            _logFolder = message;
            _processStartAt = DateTime.Now;
        }

        private void LogStartProcess()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"StartProcess - {System.Threading.Thread.CurrentThread.ManagedThreadId} - {Self.Path}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void WatchChild(IActorRef actor, string file)
        {
            Context.Watch(actor);
            _children.Add(actor.Path, file);
        }

        protected override SupervisorStrategy SupervisorStrategy()
            => new OneForOneStrategy(
                x =>
                {
                    if (x is IOException)
                    {
                        return Directive.Stop;
                    }
                    return Directive.Restart;
                });
    }

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
