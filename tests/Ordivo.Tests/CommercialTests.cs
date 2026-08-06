using Ordivo.Domain.Commercial;

namespace Ordivo.Tests;

public sealed class CommercialTests
{
    private static Plan PlanWithTrial(int days = 14) => Plan.Create("Pro", "pro", 99.90m, "BRL", BillingInterval.Monthly, days, 10, 500, 200);

    [Fact]
    public void Subscription_starts_in_trial_and_allows_access()
    {
        var now = DateTimeOffset.Parse("2026-08-06T12:00:00Z");
        var subscription = Subscription.Start(Guid.NewGuid(), PlanWithTrial(), now);
        Assert.Equal(SubscriptionStatus.Trialing, subscription.Status);
        Assert.Equal(now.AddDays(14), subscription.TrialEndsAt);
        Assert.False(subscription.BlocksAccess(now.AddDays(13)));
    }

    [Fact]
    public void Expired_trial_blocks_access()
    {
        var now = DateTimeOffset.Parse("2026-08-06T12:00:00Z");
        var subscription = Subscription.Start(Guid.NewGuid(), PlanWithTrial(1), now);
        Assert.True(subscription.BlocksAccess(now.AddDays(2)));
    }

    [Theory]
    [InlineData(SubscriptionStatus.PastDue)]
    [InlineData(SubscriptionStatus.Suspended)]
    [InlineData(SubscriptionStatus.Canceled)]
    public void Non_compliant_status_blocks_access(SubscriptionStatus status)
    {
        var now = DateTimeOffset.UtcNow; var subscription = Subscription.Start(Guid.NewGuid(), PlanWithTrial(0), now);
        if (status == SubscriptionStatus.PastDue) subscription.MarkPastDue();
        if (status == SubscriptionStatus.Suspended) subscription.Suspend();
        if (status == SubscriptionStatus.Canceled) subscription.Cancel(now);
        Assert.True(subscription.BlocksAccess(now));
    }

    [Fact]
    public void Paid_invoice_reactivates_subscription_and_starts_new_period()
    {
        var now = DateTimeOffset.UtcNow; var subscription = Subscription.Start(Guid.NewGuid(), PlanWithTrial(0), now); subscription.MarkPastDue();
        var renewedAt = now.AddDays(2); subscription.MarkActive(renewedAt.AddMonths(1), renewedAt);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(renewedAt, subscription.CurrentPeriodStartsAt);
        Assert.False(subscription.BlocksAccess(renewedAt));
    }
}
