using Polly;
using Polly.Retry;

namespace EventSourcingExample.Api.Resilience;

public class ProjectionPolicy
{
    private readonly IAsyncPolicy _policy;
    private readonly ILogger<ProjectionPolicy> _logger;

    public ProjectionPolicy(ILogger<ProjectionPolicy> logger)
    {
        _logger = logger;
        _policy = Policy
            .Handle<Exception>()
            .WaitAndRetryForeverAsync(
                _ => TimeSpan.FromSeconds(5),
                (exception, timeSpan) =>
                {
                    _logger.LogWarning($"An error occurred: {exception.Message}. Waiting {timeSpan} before continuing the loop.");
                });

    }
    public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken) =>
        await _policy.ExecuteAsync(ct => action(ct), cancellationToken);
}
