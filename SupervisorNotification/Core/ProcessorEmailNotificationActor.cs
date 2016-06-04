using Akka.Actor;

namespace Core
{
    public class ProcessorEmailNotificationDocketActor : ReceiveActor
    {
        private IActorRef emailSender;
        public ProcessorEmailNotificationDocketActor()
        {
            Receive<NotificationEnvelopment>(x =>
            {
                var message = BuildMessage(x);

                emailSender.Forward(new NotificationSms(x.NotificationSetting, message, x.DeliveryId));
            });
        }

        private string BuildMessage(NotificationEnvelopment envelopment)
        {
            return $"{GetType().Name} - {envelopment.DeliveryId} - {envelopment.Notification.Type} - {envelopment.NotificationSetting.Host}";
        }

        protected override void PreStart()
        {
            var props = Props.Create(() => new SenderEmailActor());
            emailSender = Context.ActorOf(props, "emailSender");
            base.PreStart();
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return base.SupervisorStrategy();
        }
    }

    public class ProcessorEmailNotificationSubmitEnquiryActor : ReceiveActor
    {
        private IActorRef emailSender;
        public ProcessorEmailNotificationSubmitEnquiryActor()
        {
            Receive<NotificationEnvelopment>(x =>
            {
                var message = BuildMessage(x);
                emailSender.Forward(new NotificationSms(x.NotificationSetting, message, x.DeliveryId));
            });
        }

        private string BuildMessage(NotificationEnvelopment envelopment)
        {
            return $"{GetType().Name} - {envelopment.DeliveryId} - {envelopment.Notification.Type} - {envelopment.NotificationSetting.Host}";
        }

        protected override void PreStart()
        {
            var props = Props.Create(() => new SenderEmailActor());
            emailSender = Context.ActorOf(props, "emailSender");
            base.PreStart();
        }
    }

    public class ProcessorEmailNotificationLinkActor : ReceiveActor
    {
        private IActorRef emailSender;
        public ProcessorEmailNotificationLinkActor()
        {
            Receive<NotificationEnvelopment>(x =>
            {
                var message = BuildMessage(x);
                
                emailSender.Forward(new NotificationSms(x.NotificationSetting, message, x.DeliveryId));
            });
        }

        private string BuildMessage(NotificationEnvelopment envelopment)
        {
            return $"{GetType().Name} - {envelopment.DeliveryId} - {envelopment.Notification.Type} - {envelopment.NotificationSetting.Host}";
        }

        protected override void PreStart()
        {
            var props = Props.Create(() => new SenderEmailActor());
            emailSender = Context.ActorOf(props, "emailSender");
            base.PreStart();
        }
    }
}
