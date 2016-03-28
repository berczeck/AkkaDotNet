using System;
using Akka.Actor;
using NotificationCore;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("NotificationSystemLocal"))
            {
                var consoleLogger = system.ActorOf<ConsoleLogger>("ConsoleLogger");
                
                var notification = 
                    system.ActorSelection("akka.tcp://NotificationSystem@127.0.0.1:8091/user/NotificationCoordinator")
                .ResolveOne(TimeSpan.FromSeconds(3))
                .Result;

                consoleLogger.Tell("Notification client started");
                
                for (int i = 0; i < 20; i++)
                {
                    notification.Tell(new Notification("SMS", $"Mensaje {i}"));
                }

                consoleLogger.Tell("Notificaicon SMS enviada");

                Console.ReadKey();                
            }

            Console.ReadLine();
        }
    }
}
