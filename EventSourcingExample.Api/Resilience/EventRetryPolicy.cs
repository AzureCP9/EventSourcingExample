using Polly;
using Polly.Retry;

namespace EventSourcingExample.Api.Resilience;

public class EventRetryPolicy : IRetryPolicy
{
    private readonly IAsyncPolicy _policy;

    public EventRetryPolicy() =>
        _policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    public string PolicyKey => nameof(EventRetryPolicy);

    public async Task ExecuteAsync(Func<Task> action) =>
        await _policy.ExecuteAsync(action);
    
}
