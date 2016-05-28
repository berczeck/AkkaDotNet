using Akka.Actor;

namespace Core
{
    public class SenderEmailActor : ReceiveActor
    {
        private readonly IActorRef coordinator;
        public SenderEmailActor(IActorRef coordinator)
        {
            this.coordinator = coordinator;

            Receive<NotificationEmail>(x =>
            {
                Send(x);
                coordinator.Tell(new NotificationConfirmationSuccess(x.DeliveryId));
            });
        }

        private void Send(NotificationEmail notification)
        {

        }
    }

    public class SenderSmsActor : ReceiveActor
    {
        private readonly IActorRef coordinator;
        public SenderSmsActor(IActorRef coordinator)
        {
            this.coordinator = coordinator;

            Receive<NotificationSms>(x =>
            {
                Send(x);
                coordinator.Tell(new NotificationConfirmationSuccess(x.DeliveryId));
            });
        }

        private void Send(NotificationSms notification)
        {

        }
    }
}
