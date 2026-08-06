using Ordivo.Domain.Customers;

namespace Ordivo.Tests;

public sealed class CustomerTests
{
    [Fact]
    public void Create_returns_active_customer()
    {
        var customer = Customer.Create(Guid.NewGuid(), "Customer", "123", "11999999999", "customer@test.local");

        Assert.True(customer.IsActive);
    }

    [Fact]
    public void Customer_can_be_deactivated_and_reactivated()
    {
        var customer = Customer.Create(Guid.NewGuid(), "Customer", "123", "11999999999", null);

        customer.Deactivate();
        Assert.False(customer.IsActive);

        customer.Activate();
        Assert.True(customer.IsActive);
    }
}
