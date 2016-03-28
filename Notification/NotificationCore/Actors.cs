using System;
using Akka.Actor;
using Akka.Persistence;
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

    //public class SmsSenderActor : TypedActor, IHandle<SmsNotification>
    //{
    //    private int contador = 0;
    //    public void Handle(SmsNotification message)
    //    {
    //        contador++;
    //        Context.ActorSelection("/ConsoleLogger")
    //           .Tell($"{Context.Self.Path} handle notification Sms");

    //        if (contador % 2 != 0)
    //        {
    //            throw new ApplicationException("No se puede entregar el mensaje");
    //        }
    //    }
    //}

    public class SmsSenderActor : TypedActor, IHandle<ReliableDeliveryEnvelope<SmsNotification>>
    {
        private int contador = 0;
        public void Handle(ReliableDeliveryEnvelope<SmsNotification> message)
        {
            contador++;
            Context.ActorSelection("akka://NotificationSystem/user/ConsoleLogger")
               .Tell($"{Context.Self.Path} handle notification Sms  {message.Message.Notification.Category} {message.Message.Notification.Id}");

            if (contador == 1 || contador == 7)
            {
                throw new ApplicationException("No se puede entregar el mensaje");
            }
            Context.ActorSelection("akka://NotificationSystem/user/ConsoleLogger")
             .Tell($"Mensaje procesado {message.Message.Notification.Category} {message.Message.Notification.Id}");
            Sender.Tell(new ReliableDeliveryAck(message.MessageId));
        }
    }

    //public class SmsNotificationCoordinator : TypedActor, IHandle<SmsNotification>
    //{
    //    private IActorRef smsSender;

    //    public SmsNotificationCoordinator()
    //    {
    //        var props = Props.Create<SmsSenderActor>().WithRouter(FromConfig.Instance);
    //        //var props = Props.Create<EmailSenderActor>().WithRouter(new RoundRobinPool(5));
    //        smsSender = Context.ActorOf(props, "smsSender");
    //    }

    //    public void Handle(SmsNotification message)
    //    {
    //        Context.ActorSelection("/ConsoleLogger")
    //           .Tell("SmsNotificationCoordinator handle notification Sms");

    //        smsSender.Tell(message);
    //    }

    //    protected override SupervisorStrategy SupervisorStrategy()
    //    {
    //        return new OneForOneStrategy(
    //            exception =>
    //            {
    //                if (exception is ApplicationException)
    //                {
    //                    return Directive.Resume;
    //                }

    //                return Directive.Restart;
    //            }
    //            );

    //    }
    //}

    public class SmsNotificationCoordinator : AtLeastOnceDeliveryReceiveActor
    {
        private IActorRef smsSender;
        private ICancelable recurringSnapshotCleanup;
        
        private class CleanSnapshots { }

        public SmsNotificationCoordinator()
        {
            var props = Props.Create<SmsSenderActor>().WithRouter(FromConfig.Instance);
            //var props = Props.Create<EmailSenderActor>().WithRouter(new RoundRobinPool(5));
            smsSender = Context.ActorOf(props, "smsSender");

            Recover<SnapshotOffer>(offer => offer.Snapshot is Akka.Persistence.AtLeastOnceDeliverySnapshot, offer =>
            {
                var snapshot = offer.Snapshot as Akka.Persistence.AtLeastOnceDeliverySnapshot;
                SetDeliverySnapshot(snapshot);
            });

            Command<SmsNotification>(sms =>
            {
                //Context.ActorSelection("/ConsoleLogger")
                // .Tell($"SmsNotificationCoordinator handle notification Sms {sms.Notification.Category} {sms.Notification.Id}");

                Deliver(smsSender.Path, messageId => new ReliableDeliveryEnvelope<SmsNotification>(sms, messageId));

                // save the full state of the at least once delivery actor
                // so we don't lose any messages upon crash
                SaveSnapshot(GetDeliverySnapshot());
            });

            Command<ReliableDeliveryAck>(ack =>
            {
                ConfirmDelivery(ack.MessageId);
            });

            Command<CleanSnapshots>(clean =>
            {
                // save the current state (grabs confirmations)
                SaveSnapshot(GetDeliverySnapshot());
            });

            Command<SaveSnapshotSuccess>(saved =>
            {
                var seqNo = saved.Metadata.SequenceNr;
                DeleteSnapshots(new SnapshotSelectionCriteria(seqNo, saved.Metadata.Timestamp.AddMilliseconds(-1))); // delete all but the most current snapshot
            });
        }

        public override string PersistenceId => Context.Self.Path.Name;
        
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

        protected override void PreStart()
        {
            //recurringSnapshotCleanup =
            //    Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(10),
            //        TimeSpan.FromSeconds(10), Self, new CleanSnapshots(), ActorRefs.NoSender);

            base.PreStart();
        }

        protected override void PostStop()
        {
            //recurringSnapshotCleanup?.Cancel();

            base.PostStop();
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
            consoleLogger.Tell($"NotificationCoordinator handle notification {message.Category} {message.Id}");
            if ("SMS".Equals(message.Category?.ToUpper()))
            {
                smsCoordinator.Tell(new SmsNotification(message));
            }
            else
            {
                emailCoordinator.Tell(new EMailNotification(message));
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
        public string Id { get; }
        public Notification(string category,string id)
        {
            Category = category;
            Id = id;
        }
    }
    public class EMailNotification
    {
        public Notification Notification { get; }

        public EMailNotification(Notification notification)
        {
            Notification = notification;
        }
    }
    public class SmsNotification
    {
        public Notification Notification { get; }

        public SmsNotification(Notification notification)
        {
            Notification = notification;
        }
    }
}
