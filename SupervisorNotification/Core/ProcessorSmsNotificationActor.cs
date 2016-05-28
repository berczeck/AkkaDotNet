using Akka.Actor;

namespace Core
{
    public class ProcessorSmsNotificationDocketActor : ReceiveActor
    {
        private readonly IActorRef sender;
        public ProcessorSmsNotificationDocketActor(IActorRef sender)
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

    public class ProcessorSmsNotificationKeyDocumentActor : ReceiveActor
    {
        private readonly IActorRef sender;
        public ProcessorSmsNotificationKeyDocumentActor(IActorRef sender)
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
