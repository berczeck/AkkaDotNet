using Akka.Actor;

namespace Core
{
    public class SenderEmailActor : ReceiveActor
    {
        public SenderEmailActor()
        {

            Receive<NotificationEmail>(x =>
            {
                Send(x);
                Sender.Tell(new NotificationConfirmationSuccess(x.DeliveryId));
            });
        }

        private void Send(NotificationEmail notification)
        {

        }
    }

    public class SenderSmsActor : ReceiveActor
    {
        public SenderSmsActor()
        {
            Receive<NotificationSms>(x =>
            {
                Send(x);
                Sender.Tell(new NotificationConfirmationSuccess(x.DeliveryId));
            });
        }

        private void Send(NotificationSms notification)
        {

        }
    }
}
