using Ordivo.Domain.ServiceOrders;
namespace Ordivo.Tests;
public sealed class ServiceOrderTests
{
    [Fact] public void Complete_sets_completion_date()
    { var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Repair", "Replace display", 250m); order.ChangeStatus(ServiceOrderStatus.Completed); Assert.Equal(ServiceOrderStatus.Completed, order.Status); Assert.NotNull(order.CompletedAt); }
    [Fact] public void Closed_order_cannot_change_status()
    { var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Repair", "Replace display", 250m); order.ChangeStatus(ServiceOrderStatus.Cancelled); Assert.Throws<InvalidOperationException>(() => order.ChangeStatus(ServiceOrderStatus.Open)); }
    [Fact] public void Create_raises_domain_event()
    { var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Repair", "Replace display", 250m); Assert.Single(order.DomainEvents); Assert.IsType<ServiceOrderCreatedDomainEvent>(order.DomainEvents.Single()); }
}
