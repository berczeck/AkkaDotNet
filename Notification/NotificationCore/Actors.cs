using System;
using Akka.Actor;
using Akka.Routing;

namespace NotificationCore
{
    public class EmailSenderActor : TypedActor, IHandle<EMailNotification>
    {
        public void Handle(EMailNotification message)
        {
            Context.ActorSelection("/ConsoleLogger")
               .Tell("EmailSenderActor handle notification Email");
        }
    }

    public class EmailNotificationCoordinator : TypedActor, IHandle<EMailNotification>
    {
        private IActorRef emailSender;

        public EmailNotificationCoordinator()
        {
            var props = Props.Create<EmailSenderActor>().WithRouter(FromConfig.Instance);

            //var props = Props.Create<EmailSenderActor>().WithRouter(new RoundRobinPool(5));
            emailSender = Context.ActorOf(props, "emailSender");
        }

        public void Handle(EMailNotification message)
        {
            Context.ActorSelection("/ConsoleLogger")
               .Tell("EmailNotificationCoordinator handle notification Email");

            emailSender.Tell(message);
        }
    }

    public class SmsSenderActor : TypedActor, IHandle<SmsNotification>
    {
        private int contador = 0;
        public void Handle(SmsNotification message)
        {
            contador++;
            Context.ActorSelection("/ConsoleLogger")
               .Tell($"{Context.Self.Path} handle notification Sms");

            if (contador % 2 != 0)
            {
                throw new ApplicationException("No se puede entregar el mensaje");
            }
        }
    }

    public class SmsNotificationCoordinator : TypedActor, IHandle<SmsNotification>
    {
        private IActorRef smsSender;

        public SmsNotificationCoordinator()
        {
            var props = Props.Create<SmsSenderActor>().WithRouter(FromConfig.Instance);
            //var props = Props.Create<EmailSenderActor>().WithRouter(new RoundRobinPool(5));
            smsSender = Context.ActorOf(props, "smsSender");
        }

        public void Handle(SmsNotification message)
        {
            Context.ActorSelection("/ConsoleLogger")
               .Tell("SmsNotificationCoordinator handle notification Sms");

            smsSender.Tell(message);
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                exception =>
                {
                    if (exception is ApplicationException)
                    {
                        return Directive.Resume;
                    }

                    return Directive.Restart;
                }
                );

        }
    }

    public class NotificationCoordinator : TypedActor, IHandle<Notification>
    {
        private IActorRef consoleLogger;
        private IActorRef emailCoordinator;
        private IActorRef smsCoordinator;

        public NotificationCoordinator(IActorRef consoleLogger)
        {
            this.consoleLogger = consoleLogger;
            emailCoordinator = Context.ActorOf<EmailNotificationCoordinator>("EmailNotificationCoordinator");
            smsCoordinator = Context.ActorOf<SmsNotificationCoordinator>("SmsNotificationCoordinator");
        }

        public void Handle(Notification message)
        {
            consoleLogger.Tell($"NotificationCoordinator handle notification {message.Category}");
            if ("SMS".Equals(message.Category?.ToUpper()))
            {
                smsCoordinator.Tell(new SmsNotification());
            }
            else
            {
                emailCoordinator.Tell(new EMailNotification());
            }
        }
    }

    public class ConsoleLogger : ReceiveActor
    {
        public ConsoleLogger()
        {
            Receive<string>(x => Console.WriteLine(x));
        }
    }

    public class Notification
    {
        public string Category { get; }
        public Notification(string category)
        {
            Category = category;
        }
    }
    public class EMailNotification { }
    public class SmsNotification { }
}
