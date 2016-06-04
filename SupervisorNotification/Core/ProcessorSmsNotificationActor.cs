using Akka.Actor;
using System;

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
            return new OneForOneStrategy(
                maxNrOfRetries: 10,
                withinTimeRange: TimeSpan.FromSeconds(30),
                localOnlyDecider: x =>
                {
                    // Maybe ArithmeticException is not application critical
                    // so we just ignore the error and keep going.
                    if (x is ArithmeticException) return Directive.Resume;
                    // Error that we have no idea what to do with
                    else if (x is NotImplementedException) return Directive.Escalate;
                    // Error that we can't recover from, stop the failing child
                    else if (x is NotSupportedException) return Directive.Stop;
                    // otherwise restart the failing child
                    else return Directive.Restart;
                });
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
            return new OneForOneStrategy(
                maxNrOfRetries: 10,
                withinTimeRange: TimeSpan.FromSeconds(30),
                localOnlyDecider: x =>
                {
                    // Maybe ArithmeticException is not application critical
                    // so we just ignore the error and keep going.
                    if (x is ArithmeticException) return Directive.Resume;
                    // Error that we have no idea what to do with
                    else if (x is NotImplementedException) return Directive.Escalate;
                    // Error that we can't recover from, stop the failing child
                    else if (x is NotSupportedException) return Directive.Stop;
                    // otherwise restart the failing child
                    else return Directive.Restart;
                });
        }
    }
}
