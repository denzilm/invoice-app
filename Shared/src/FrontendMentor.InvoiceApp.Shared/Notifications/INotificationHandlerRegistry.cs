namespace FrontendMentor.InvoiceApp.Shared.Notifications;

public interface INotificationHandlerRegistry
{
    IReadOnlyList<Type> GetHandlersForNotification(Type notificationType);
}
