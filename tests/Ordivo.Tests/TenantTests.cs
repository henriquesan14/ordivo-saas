using Ordivo.Domain.Tenants;

namespace Ordivo.Tests;

public sealed class TenantTests
{
    [Fact]
    public void Create_returns_active_tenant()
    {
        var tenant = Tenant.Create("Ordivo Demo");

        Assert.NotEqual(Guid.Empty, tenant.Id);
        Assert.Equal("Ordivo Demo", tenant.Name);
        Assert.True(tenant.IsActive);
    }

    [Fact]
    public void Rename_changes_tenant_name()
    {
        var tenant = Tenant.Create("Old name");

        tenant.Rename("New name");

        Assert.Equal("New name", tenant.Name);
    }
}
