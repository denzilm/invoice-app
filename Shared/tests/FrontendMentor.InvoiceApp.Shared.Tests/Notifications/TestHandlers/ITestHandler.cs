using FrontendMentor.InvoiceApp.Shared.Notifications;
using FrontendMentor.InvoiceApp.Shared.Tests.Notifications.TestNotifications;

namespace FrontendMentor.InvoiceApp.Shared.Tests.Notifications.TestHandlers;

public interface ITestHandler : INotificationHandler<TestNotification>;
