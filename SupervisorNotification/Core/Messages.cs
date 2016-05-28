using System;

namespace Core
{
    public class SendEmailNotification : Notification
    {
        public SendEmailNotification(int docketId, int projectId, string type):base(docketId, projectId,type)
        {
        }
    }
    public class SendSmsNotification : Notification
    {
        public SendSmsNotification(int docketId, int projectId, string type) : base(docketId, projectId, type)
        {
        }
    }

    public class Notification
    {
        public int DocketId { get; }
        public int ProjectId { get; }
        public string Type { get; }

        public Notification(int docketId, int projectId, string type)
        {
            DocketId = docketId;
            ProjectId = projectId;
            Type = type;
        }
    }

    public class NotificationEnvelopment
    {
        public NotificationSetting NotificationSetting { get; }
        public Notification Notification { get; }
        public long DeliveryId { get; }

        public NotificationEnvelopment(NotificationSetting notificationSetting, Notification notification, long deliveryId)
        {
            NotificationSetting = notificationSetting;
            Notification = notification; ;
            DeliveryId = deliveryId;
        }
    }

    public enum NotificationSettingType
    {
        Email,
        Sms
    }
    public class NotificationSetting
    {
        public NotificationSettingType Type { get; }
        public string Host { get; }
        public string Account { get; }
        public string Password { get; }
        public int Capacity { get; }
        public Guid Identifier { get; }

        public NotificationSetting(NotificationSettingType type, Guid identifier, string host, string account, string password, int capacity)
        {
            Host = host;
            Account = account;
            Password = password;
            Type = type;
            Capacity = capacity;
            Identifier = identifier;
        }
    }

    public class NotificationEmail
    {
        public NotificationSetting NotificationSetting { get; }
        public string Body { get; }
        public long DeliveryId { get; }

        public NotificationEmail(NotificationSetting notificationSetting, string body, long deliveryId)
        {
            DeliveryId = deliveryId;
            NotificationSetting = notificationSetting;
            Body = body;
        }
    }

    public class NotificationSms
    {
        public NotificationSetting NotificationSetting { get; }
        public string Body { get; }
        public long DeliveryId { get; }

        public NotificationSms(NotificationSetting notificationSetting, string body, long deliveryId)
        {
            DeliveryId = deliveryId;
            NotificationSetting = notificationSetting;
            Body = body;
        }
    }

    public class NotificationConfirmationSuccess
    {
        public long DeliveryId { get; }

        public NotificationConfirmationSuccess(long deliveryId)
        {
            DeliveryId = deliveryId;
        }
    }

    public class NotificationConfirmationError
    {
        public long DeliveryId { get; }
        public Guid NottificationSettingIdentifier { get; }

        public NotificationConfirmationError(Guid nottificationSettingIdentifier, long deliveryId)
        {
            DeliveryId = deliveryId;
            NottificationSettingIdentifier = nottificationSettingIdentifier;
        }
    }
}
