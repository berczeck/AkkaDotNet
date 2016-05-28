using Akka.Actor;

namespace Core
{
    public class ProcessorEmailNotificationDocketActor : ReceiveActor
    {
        private readonly IActorRef sender;
        public ProcessorEmailNotificationDocketActor(IActorRef sender)
        {
            this.sender = sender;

            Receive<NotificationEnvelopment>(x =>
            {
                var message = BuildMessage(x);

                sender.Tell(new NotificationSms(x.NotificationSetting, message, x.DeliveryId));
            });
        }

        private string BuildMessage(NotificationEnvelopment envelopment)
        {
            return $"{GetType().Name} - {envelopment.DeliveryId} - {envelopment.Notification.Type} - {envelopment.NotificationSetting.Host}";
        }
    }

    public class ProcessorEmailNotificationSubmitEnquiryActor : ReceiveActor
    {
        private readonly IActorRef sender;
        public ProcessorEmailNotificationSubmitEnquiryActor(IActorRef sender)
        {
            this.sender = sender;

            Receive<NotificationEnvelopment>(x =>
            {
                var message = BuildMessage(x);

                sender.Tell(new NotificationSms(x.NotificationSetting, message, x.DeliveryId));
            });
        }

        private string BuildMessage(NotificationEnvelopment envelopment)
        {
            return $"{GetType().Name} - {envelopment.DeliveryId} - {envelopment.Notification.Type} - {envelopment.NotificationSetting.Host}";
        }
    }

    public class ProcessorEmailNotificationLinkActor : ReceiveActor
    {
        private readonly IActorRef sender;
        public ProcessorEmailNotificationLinkActor(IActorRef sender)
        {
            this.sender = sender;

            Receive<NotificationEnvelopment>(x =>
            {
                var message = BuildMessage(x);

                sender.Tell(new NotificationSms(x.NotificationSetting, message, x.DeliveryId));
            });
        }

        private string BuildMessage(NotificationEnvelopment envelopment)
        {
            return $"{GetType().Name} - {envelopment.DeliveryId} - {envelopment.Notification.Type} - {envelopment.NotificationSetting.Host}";
        }
    }
}
