using System;
using Akka.Actor;
using NotificationCore;

namespace Notification
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("NotificationSystem"))
            {
               var consoleLogger = system.ActorOf<ConsoleLogger>("ConsoleLogger");
                //system.ActorOf<NotificationCoordinator>("NotificationCoordinator");
                consoleLogger.Tell("Notification server started");
                var notification= system.ActorOf(Props.Create(() => new NotificationCoordinator(consoleLogger)), "NotificationCoordinator");

                //notification.Tell(new NotificationCore.Notification("SMS"));
                //notification.Tell(new NotificationCore.Notification("SMS"));

                Console.ReadKey();

                //system.WhenTerminated.Wait();
            }

            Console.ReadLine();
        }
    }
}
