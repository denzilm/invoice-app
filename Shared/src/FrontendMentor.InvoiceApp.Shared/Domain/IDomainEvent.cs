namespace FrontendMentor.InvoiceApp.Shared.Domain;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
