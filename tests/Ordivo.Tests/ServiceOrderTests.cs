using Ordivo.Domain.ServiceOrders;
namespace Ordivo.Tests;
public sealed class ServiceOrderTests
{
    private static ServiceOrder Create() => ServiceOrder.Create(Guid.NewGuid(),123,Guid.NewGuid(),"Repair","Replace display",250m,null,null,"Tester",DateTimeOffset.UtcNow);
    [Fact] public void Complete_sets_completion_and_history(){var order=Create();order.ChangeStatus(ServiceOrderStatus.Completed,"Tester",DateTimeOffset.UtcNow,"Done");Assert.NotNull(order.CompletedAt);Assert.Equal(2,order.StatusHistory.Count);}
    [Fact] public void Closed_order_cannot_change(){var order=Create();order.ChangeStatus(ServiceOrderStatus.Cancelled,"Tester",DateTimeOffset.UtcNow);Assert.Throws<InvalidOperationException>(()=>order.ChangeStatus(ServiceOrderStatus.Open,"Tester",DateTimeOffset.UtcNow));}
    [Fact] public void Create_generates_friendly_number(){var order=Create();Assert.Equal($"OS-{DateTimeOffset.UtcNow.Year}-000123",order.Number);Assert.Single(order.DomainEvents);}
    [Fact] public void Comments_and_attachments_are_added(){var order=Create();var user=Guid.NewGuid();order.AddComment(user,"Tester","Comment",DateTimeOffset.UtcNow);order.AddAttachment(user,"Tester","file.pdf","application/pdf",10,"orders/file.pdf",DateTimeOffset.UtcNow);Assert.Single(order.Comments);Assert.Single(order.Attachments);}
    [Fact] public void Attachment_can_be_removed(){var order=Create();var user=Guid.NewGuid();order.AddAttachment(user,"Tester","file.pdf","application/pdf",10,"orders/file.pdf",DateTimeOffset.UtcNow);var attachment=order.Attachments.Single();Assert.NotNull(order.RemoveAttachment(attachment.Id));Assert.Empty(order.Attachments);}
}
