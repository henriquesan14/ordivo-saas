using Ordivo.Domain.Impersonation;

namespace Ordivo.Tests;
public sealed class ImpersonationSessionTests
{
    [Fact]
    public void Session_is_active_only_inside_its_window()
    {
        var now=DateTimeOffset.Parse("2026-08-06T12:00:00Z");
        var session=ImpersonationSession.Start(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),"Support ticket 123",now,TimeSpan.FromMinutes(15));
        Assert.True(session.IsActive(now.AddMinutes(14)));
        Assert.False(session.IsActive(now.AddMinutes(16)));
        Assert.Equal("Support ticket 123",session.Reason);
    }
    [Fact]
    public void End_revokes_session_immediately()
    {
        var now=DateTimeOffset.UtcNow;var session=ImpersonationSession.Start(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),"Investigate order issue",now,TimeSpan.FromMinutes(15));
        session.End(now.AddMinutes(2));
        Assert.False(session.IsActive(now.AddMinutes(3)));
        Assert.Equal(now.AddMinutes(2),session.EndedAt);
    }
}
