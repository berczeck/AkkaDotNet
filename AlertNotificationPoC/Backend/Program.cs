using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Akka.Actor;
using Akka.Routing;
using Akka.Util.Internal;
using Messages;

namespace Backend
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("demo"))
            {
                var analyzer = system.ActorOf<AnalyzerCoordinatorActor>($"{nameof(AnalyzerCoordinatorActor)}");
                Thread.Sleep(2000);
                analyzer.Tell(new StartAnalysis());
                Console.WriteLine("Analysis started");
                Console.ReadLine();
            }
        }
    }

    public class AnalyzerCoordinatorActor : ReceiveActor
    {
        private readonly string _name =
            $"{nameof(AnalyzerCoordinatorActor)}_{DateTime.Now.ToString("yyyyMMddhhmmssms")}";

        private readonly Stopwatch _stopwatch = new Stopwatch();

        public AnalyzerCoordinatorActor()
        {
            Receive<StartAnalysis>(x =>
            {
                var analyzer = Context.ActorOf<AnalyzerActor>(_name);
                Context.Watch(analyzer);
                analyzer.Tell(x);
                _stopwatch.Start();
                Console.WriteLine($"Start time: {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}");
            });
            Receive<FinishAnalysis>(x =>
            {
                _stopwatch.Stop();
                Console.WriteLine($"End time: {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}");
                Console.WriteLine($"Process time: {_stopwatch.Elapsed}");
                Self.Tell(PoisonPill.Instance);
            });
            Receive<Terminated>(x =>
            {
                Self.Tell(new FinishAnalysis());
            });
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return base.SupervisorStrategy();
        }
    }

    public class AnalyzerActor : ReceiveActor
    {
        private CategoryAnalyzed _categoryAnalyzed;
        private KeywordAnalyzed _keywordAnalyzed;
        public AnalyzerActor()
        {
            SetReceiveTimeout(TimeSpan.FromSeconds(30));
            Receive<StartAnalysis>(x => HandleStartAnalysis(x));
            Receive<CategoryAnalyzed>(x => HandleCategoryAnalyzed(x));
            Receive<KeywordAnalyzed>(x => HandleKeywordAnalyzed(x));
            Receive<AnalysisFinished>(x => HandleAnalysisFinished(x));
            Receive<SendRealTimeNotification>(x => HandleSendRealTimeNotification(x));
            Receive<ReceiveTimeout>(x => HandleReceiveTimeout(x));
            Receive<Terminated>(x =>
            {
                if (!Context.GetChildren().Any())
                {
                    Self.Tell(PoisonPill.Instance);
                }
            });
        }

        private void HandleReceiveTimeout(ReceiveTimeout message)
        {
            Self.Tell(PoisonPill.Instance);
        }

        private void HandleSendRealTimeNotification(SendRealTimeNotification message)
        {
            //Notifications query
            Thread.Sleep(2000);
            var emailNotifications = Enumerable.Range(0, Constant.MessageCount).Select(x => new SendEmail {Index = x});
            var smsNotifications = Enumerable.Range(0, Constant.MessageCount).Select(x => new SendSms { Index = x });
            Trace.Log("Notifications fetched from database");

            var emailProps = Props.Create<EmailSenderActor>().WithRouter(new RoundRobinPool(Constant.WorkerCount));
            var emailSender = Context.ActorOf(emailProps, $"{nameof(EmailSenderActor)}");
            var smsProps = Props.Create<SmsSenderActor>().WithRouter(new RoundRobinPool(Constant.WorkerCount));
            var smsSender = Context.ActorOf(smsProps,$"{nameof(SmsSenderActor)}");
            Context.Watch(emailSender);
            Context.Watch(smsSender);
            emailNotifications.ForEach(x => emailSender.Tell(x));
            smsNotifications.ForEach(x => smsSender.Tell(x));
        }

        private void HandleAnalysisFinished(AnalysisFinished message)
        {
            //Save to database
            Thread.Sleep(700);
            Trace.Log("Records saved in database");
            Self.Tell(new SendRealTimeNotification());
        }

        private void HandleStartAnalysis(StartAnalysis message)
        {
            Trace.Log("Start Analysis");
            var categoryAnalyzer = Context.ActorOf<CategoryAnalyzerActor>($"{nameof(CategoryAnalyzerActor)}");
            var keywordAnalyzer = Context.ActorOf<KeywordAnalyzerActor>($"{nameof(KeywordAnalyzerActor)}");
            categoryAnalyzer.Tell(message);
            keywordAnalyzer.Tell(message);
        }

        private void HandleCategoryAnalyzed(CategoryAnalyzed message)
        {
            _categoryAnalyzed = message;
            SendAnalysisResult();
        }

        private void HandleKeywordAnalyzed(KeywordAnalyzed message)
        {
            _keywordAnalyzed = message;
            SendAnalysisResult();
        }

        private void SendAnalysisResult()
        {
            if (_keywordAnalyzed == null || _categoryAnalyzed == null) return;
            var result = new AnalysisFinished {Result = new List<AnalyzeDetail>()};
            result.Result.AddRange(_keywordAnalyzed.Result);
            result.Result.AddRange(_categoryAnalyzed.Result);
            Self.Tell(result);
            Trace.Log("Full Analysis finished");
        }
    }

    public class CategoryAnalyzerActor : ReceiveActor
    {
        public CategoryAnalyzerActor()
        {
            Receive<StartAnalysis>(x =>
            {
                Thread.Sleep(1000);
                Trace.Log("Category analyzer finished");
                Sender.Tell(new CategoryAnalyzed {Result = new List<AnalyzeDetail>()});
                Self.Tell(PoisonPill.Instance);
            });
        }
    }

    public class KeywordAnalyzerActor : ReceiveActor
    {
        public KeywordAnalyzerActor()
        {
            Receive<StartAnalysis>(x =>
            {
                Thread.Sleep(2000);
                Trace.Log("Keyword analyzer finished");
                Sender.Tell(new KeywordAnalyzed {Result = new List<AnalyzeDetail>()});
                Self.Tell(PoisonPill.Instance);
            });
        }
    }

    public class SmsSenderActor : ReceiveActor
    {
        public SmsSenderActor()
        {
            SetReceiveTimeout(TimeSpan.FromSeconds(1));
            Receive<SendSms>(x =>
            {
                Thread.Sleep(200);
                Trace.Log("Sms notification processed");
            });
            Receive<ReceiveTimeout>(x => Self.Tell(PoisonPill.Instance));
        }
    }

    public class EmailSenderActor : ReceiveActor
    {
        public EmailSenderActor()
        {
            SetReceiveTimeout(TimeSpan.FromSeconds(1));
            Receive<SendEmail>(x =>
            {
                Thread.Sleep(300);
                Trace.Log("Email noticication processed");
            });
            Receive<ReceiveTimeout>(x => Self.Tell(PoisonPill.Instance));
        }
    }

    public static class Trace
    {
        public static void Log(string message)
        {
            Console.WriteLine($"{message} - Thread:{Thread.CurrentThread.ManagedThreadId} {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}");
        }
    }
}
