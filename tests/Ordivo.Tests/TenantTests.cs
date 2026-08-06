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
        Assert.StartsWith("ordivo-demo-", tenant.Slug);
        Assert.True(tenant.IsActive);
    }

    [Fact]
    public void Rename_changes_tenant_name()
    {
        var tenant = Tenant.Create("Old name");

        tenant.Rename("New name");

        Assert.Equal("New name", tenant.Name);
        Assert.StartsWith("old-name-", tenant.Slug);
    }

    [Fact]
    public void Tenant_can_be_suspended_and_reactivated()
    {
        var tenant = Tenant.Create("Tenant status");

        tenant.Deactivate();
        Assert.False(tenant.IsActive);

        tenant.Activate();
        Assert.True(tenant.IsActive);
    }

    [Fact]
    public void Create_normalizes_accents_and_generates_unique_slugs()
    {
        var first = Tenant.Create("Assistência Técnica São José");
        var second = Tenant.Create("Assistência Técnica São José");

        Assert.StartsWith("assistencia-tecnica-sao-jose-", first.Slug);
        Assert.NotEqual(first.Slug, second.Slug);
    }
}
