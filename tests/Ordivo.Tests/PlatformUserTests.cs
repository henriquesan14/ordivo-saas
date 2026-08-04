using Ordivo.Domain.PlatformUsers;

namespace Ordivo.Tests;

public sealed class PlatformUserTests
{
    [Fact]
    public void Create_platform_admin_has_no_tenant()
    {
        var user = PlatformUser.Create(
            "Global Admin",
            " ADMIN@ORDIVO.LOCAL ",
            "hashed-password",
            PlatformRole.PlatformAdmin);

        Assert.Equal("admin@ordivo.local", user.Email);
        Assert.Equal(PlatformRole.PlatformAdmin, user.Role);
        Assert.True(user.IsActive);
        Assert.DoesNotContain(
            typeof(Ordivo.SharedKernel.Domain.ITenantEntity),
            user.GetType().GetInterfaces());
    }
}
