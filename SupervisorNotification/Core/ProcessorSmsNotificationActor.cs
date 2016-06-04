using Akka.Actor;

namespace Core
{
    public class ProcessorSmsNotificationDocketActor : ReceiveActor
    {
        private IActorRef smsSender;
        public ProcessorSmsNotificationDocketActor()
        {
            Receive<NotificationEnvelopment>(x =>
            {
                var message = BuildMessage(x);

                smsSender.Forward(new NotificationSms(x.NotificationSetting, message, x.DeliveryId));
            });
        }

        private string BuildMessage(NotificationEnvelopment envelopment)
        {
            return $"{GetType().Name} - {envelopment.DeliveryId} - {envelopment.Notification.Type} - {envelopment.NotificationSetting.Host}";
        }

        protected override void PreStart()
        {
            var props = Props.Create(() => new SenderSmsActor());
            smsSender = Context.ActorOf(props, "emailSender");
            base.PreStart();
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return base.SupervisorStrategy();
        }
    }

    public class ProcessorSmsNotificationKeyDocumentActor : ReceiveActor
    {
        private IActorRef smsSender;
        public ProcessorSmsNotificationKeyDocumentActor()
        {
            Receive<NotificationEnvelopment>(x =>
            {
                var message = BuildMessage(x);

                smsSender.Forward(new NotificationSms(x.NotificationSetting, message, x.DeliveryId));
            });
        }

        private string BuildMessage(NotificationEnvelopment envelopment)
        {
            return $"{GetType().Name} - {envelopment.DeliveryId} - {envelopment.Notification.Type} - {envelopment.NotificationSetting.Host}";
        }

        protected override void PreStart()
        {
            var props = Props.Create(() => new SenderSmsActor());
            smsSender = Context.ActorOf(props, "emailSender");
            base.PreStart();
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return base.SupervisorStrategy();
        }
    }
}
