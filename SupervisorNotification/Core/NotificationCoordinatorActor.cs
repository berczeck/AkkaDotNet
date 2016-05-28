using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Persistence;

namespace Core
{
    public class NotificationCounterManager
    {
        public IList<NotificationCounter> NotificationCounters { get; }

        public NotificationCounterManager(IList<NotificationCounter> notificationCounters)
        {
            NotificationCounters = notificationCounters;
        }

        public NotificationCounter GetNotificationSettingAvailable(NotificationSettingType type)
        {
            var notificationSetting = NotificationCounters.Where(x => x.NotificationSetting.Type == type && x.QuantityMessagesSent < x.NotificationSetting.Capacity).FirstOrDefault();
            return notificationSetting;
        }

        public NotificationCounter GetNotificationSettingById(Guid identifier)
        {
            var notificationSetting = NotificationCounters.Where(x => x.NotificationSetting.Identifier == identifier).FirstOrDefault();
            return notificationSetting;
        }
    }
    
    public class NotificationCounter
    {
        public NotificationSetting NotificationSetting { get; }
        public int QuantityMessagesSent { get; private set; }

        public NotificationCounter(NotificationSetting notificationSetting)
        {
            NotificationSetting = notificationSetting;
        }

        public void IncreaseCounter()
        {
            QuantityMessagesSent++;
        }

        public void DecreaseCounter()
        {
            QuantityMessagesSent--;
        }
    }

    public class LoadNotificaitonSetting { }
    public class ResetNotificaitonSetting { }

    public class NotificationCoordinatorActor : AtLeastOnceDeliveryReceiveActor
    {
        public override string PersistenceId => Context.Self.Path.Name;
        private Dictionary<string, IActorRef> Procesors { get; set; }

        private NotificationCounterManager NotificationCounterManager { get; set; }

        public NotificationCoordinatorActor()
        {
            Command<NotificationConfirmationSuccess>(x => ConfirmDelivery(x.DeliveryId));
            Command<NotificationConfirmationError>(x =>
            {
                ConfirmDelivery(x.DeliveryId);
                var notificationSettign = NotificationCounterManager.GetNotificationSettingById(x.NottificationSettingIdentifier);
                notificationSettign.DecreaseCounter();
                PersisteMessage(x);
            });
            Command<SendEmailNotification>(x =>
            {
                var setting = NotificationCounterManager.GetNotificationSettingAvailable(NotificationSettingType.Email);
                if(setting == null)
                {
                    // TODO: Become: persist all email mesages until reset
                    PersisteMessage(x);
                    return;
                }

                var processor = Procesors[x.Type];
                Deliver(processor.Path, messageId => new NotificationEnvelopment(setting.NotificationSetting, x, messageId));

                setting.IncreaseCounter();
            });

            Command<SendSmsNotification>(x =>
            {
                var setting = NotificationCounterManager.GetNotificationSettingAvailable(NotificationSettingType.Sms);
                if (setting == null)
                {
                    // TODO: Become: persist all sms mesages until reset
                    PersisteMessage(x);
                    return;
                }

                var processor = Procesors[x.Type];
                Deliver(processor.Path, messageId => new NotificationEnvelopment(setting.NotificationSetting, x, messageId));
                setting.IncreaseCounter();
            });

            Command<LoadNotificaitonSetting>(x => LoadNotificationSetting());
            Command<ResetNotificaitonSetting>(x => TryRestNotificationSetting());
        }

        private void PersisteMessage<T>(T message)
        {
            // TODO: Persist message
        }

        private void LoadNotificationSetting()
        {
            // TODO: Load settings
            NotificationCounterManager = new NotificationCounterManager(null);
        }

        private void TryRestNotificationSetting()
        {
            if (DateTime.Now.Hour == 0)
            {
                Self.Tell(new LoadNotificaitonSetting());

                var intMinutesUntilNextDay = 0;
                Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromMinutes(intMinutesUntilNextDay), Self, new ResetNotificaitonSetting(), Self);
            }
        }

        protected override void PreStart()
        {
            Procesors = new Dictionary<string, IActorRef>();
            var notificaitonSmsSender = Context.ActorOf(Props.Create(() => new SenderSmsActor(Self)));
            var notificaitonEmailSender = Context.ActorOf(Props.Create(() => new SenderEmailActor(Self)));

            var smsDocketNotification = Context.ActorOf(Props.Create(() => new ProcessorSmsNotificationDocketActor(notificaitonSmsSender)));
            var smsKeyDocumentNotification = Context.ActorOf(Props.Create(() => new ProcessorSmsNotificationKeyDocumentActor(notificaitonSmsSender)));


            var emailDocketNotification = Context.ActorOf(Props.Create(() => new ProcessorEmailNotificationDocketActor(notificaitonEmailSender)));
            var emailLinkNotification = Context.ActorOf(Props.Create(() => new ProcessorEmailNotificationLinkActor(notificaitonEmailSender)));
            var emailEnquiryNotification = Context.ActorOf(Props.Create(() => new ProcessorEmailNotificationSubmitEnquiryActor(notificaitonEmailSender)));

            Procesors.Add(Constants.SmsDocket, smsDocketNotification);
            Procesors.Add(Constants.SmsKeyDocument, smsKeyDocumentNotification);
            Procesors.Add(Constants.EmailDocket, emailDocketNotification);
            Procesors.Add(Constants.EmailLink, emailLinkNotification);
            Procesors.Add(Constants.EmailEnquiry, emailEnquiryNotification);

            Self.Tell(new LoadNotificaitonSetting());

            // TODO: Calculate 
            var intMinutesUntilNextDay = 0;
            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromMinutes(intMinutesUntilNextDay), Self, new ResetNotificaitonSetting(), Self);

            base.PreStart();
        }
    }
}
