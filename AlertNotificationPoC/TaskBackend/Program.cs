using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Messages;

namespace TaskBackend
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            var _stopwatch = new Stopwatch();
            _stopwatch.Start();
            Console.WriteLine($"Start time: {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}");

            var taskAnalyseCategory = Task.Factory.StartNew(AnalyseCategory);
            var taskAnalyseKeyword = Task.Factory.StartNew(AnalyseKeyword);

            Task.WaitAll(taskAnalyseKeyword, taskAnalyseCategory);

            var result = new AnalysisFinished { Result = new List<AnalyzeDetail>() };
            result.Result.AddRange(taskAnalyseCategory.Result.Result);
            result.Result.AddRange(taskAnalyseKeyword.Result.Result);
            Log("Full Analysis finished");

            SaveRecords();

            SendRealTimeNotification();
            _stopwatch.Stop();
            Console.WriteLine($"End time: {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}");
            Console.WriteLine($"Process time: {_stopwatch.Elapsed}");
            Console.ReadLine();
        }

        private static void SendRealTimeNotification()
        {
            Thread.Sleep(2000);
            var emailNotifications = Enumerable.Range(0, Constant.MessageCount).Select(x => new SendEmail { Index = x });
            var smsNotifications = Enumerable.Range(0, Constant.MessageCount).Select(x => new SendSms { Index = x });
            Log("Notifications fetched from database");

            var notificationBag = new List<object>();
            notificationBag.AddRange(smsNotifications.ToArray());
            notificationBag.AddRange(emailNotifications.ToArray());
            
            Parallel.ForEach(notificationBag, new ParallelOptions { MaxDegreeOfParallelism = Constant.WorkerCount },x =>
            {
                if (x is SendEmail)
                {
                    SendEmail(x as SendEmail);
                }
                else
                {
                    SendSms(x as SendSms);
                }
            });
        }
        
        private static void SendSms(SendSms x)
        {
            Thread.Sleep(200);
            Log($"Sms notification processed id {x.Index}");
        }

        private static void SendEmail(SendEmail x)
        {
            Thread.Sleep(300);
            Log($"Email noticication processed id {x.Index}");
        }

        private static void SaveRecords()
        {
            Thread.Sleep(700);
            Log("Records saved in database");
        }

        private static CategoryAnalyzed AnalyseCategory()
        {
            Thread.Sleep(1000);
            Log("Category analyzer finished");
            return new CategoryAnalyzed {Result = new List<AnalyzeDetail>()};
        }

        private static KeywordAnalyzed AnalyseKeyword()
        {
            Thread.Sleep(2000);
            Log("Keyword analyzer finished");
            return new KeywordAnalyzed {Result = new List<AnalyzeDetail>()};
        }

        private static void Log(string message)
        {
            Console.WriteLine($"{message} - Thread:{Thread.CurrentThread.ManagedThreadId} {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}");
        }
    }
}
