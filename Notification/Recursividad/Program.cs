using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Routing;

namespace Recursividad
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("AwaitForNotificationSystem"))
            {
                system.ActorOf<UpdateModelListener>("UpdateModelListener");
                system.ActorOf<RealTimeNotificationListener>("RealTimeNotificationListener");
                
                var actors = new[] { "/user/RealTimeNotificationListener", "/user/UpdateModelListener" };
                var router = system.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(actors)), "notificationChanges");
                
                system.ActorOf(Props.Create(() => new ServiceBrokerWatcher(router)), "ServiceBrokerWatcher");

                Console.ReadKey();                
            }

        }
    }

    public class UpdateModelListener : TypedActor, IHandle<Message>
    {
        public void Handle(Message message)
        {
            if (DateTime.Now.Second % 2 == 0)
            {
                throw new Exception("Error");
            }
            Console.WriteLine($"UpdateModelListener watch message: {message.Contador}");
        }
    }

    public class RealTimeNotificationListener : TypedActor, IHandle<Message>
    {
        public void Handle(Message message)
        {
            Console.WriteLine($"RealTimeNotificationListener watch message: {message.Contador}");
        }
    }

    public class ServiceBrokerWatcher : TypedActor, IHandle<WatchChanges>, IHandle<Message>
    {
        private int contador;
        private IActorRef router;

        public ServiceBrokerWatcher(IActorRef router)
        {
            this.router = router;
        }

        public void Handle(Message message)
        {
            router.Tell(message);
            Console.WriteLine($"End watch message: {message.Contador}");
            Self.Tell(new WatchChanges());
        }

        public void Handle(WatchChanges watchChanges)
        {
            contador++;
            Console.WriteLine($"Start watch message: {contador}");
            GetMessage().PipeTo(Self);
        }

        protected override void PreStart()
        {
            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(5), Self, new WatchChanges(), ActorRefs.NoSender);
            base.PreStart();
        }

        private async Task<Message> GetMessage()
        {
            if (DateTime.Now.Second % 2 == 0)
            {
                System.Threading.Thread.Sleep(5000);
            }
            else
            {
                System.Threading.Thread.Sleep(3000);
            }

            return await Task.FromResult(new Message(contador));
        }
    }

    public class WatchChanges { }
    public class Message {
        public int Contador { get; }

        public Message(int contador)
        {
            Contador = contador;
        }
    }
}
