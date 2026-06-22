using Hangfire;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.Hangfire.Jobs;

public class SubscriptionExpirationJob(ISubscriptionService subscriptionService)
{
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessExpiredSubscription(CancellationToken cancellationToken)
    {
        await subscriptionService.ProcessExpiredSubscriptions(cancellationToken);
    }
}
