using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;
using Polly;
using Polly.Retry;
using Statiq.Common;

namespace Statiq.Web.GitHub
{
    public static class GitHubClientExtensions
    {
        private const int RetryCount = 5;

        private static readonly AsyncRetryPolicy _retryPolicy = Policy
            .Handle<ApiException>()
            .WaitAndRetryAsync(
                RetryCount,
                (_, ex, __) =>
                {
                    return ex switch
                    {
                        AbuseException abuseEx => TimeSpan.FromSeconds(abuseEx.RetryAfterSeconds is object
                            ? abuseEx.RetryAfterSeconds.Value * 1.5
                            : 30),
                        RateLimitExceededException rateEx => TimeSpan.FromSeconds(
                            (rateEx.Reset - DateTimeOffset.Now).TotalSeconds * 1.5),
                        _ => TimeSpan.FromSeconds(30)
                    };
                },
                (ex, ts, retry, ctx) =>
                {
                    IExecutionContext context = (IExecutionContext)ctx[nameof(IExecutionContext)];
                    context.LogWarning($"GitHub exception {ex.GetType().Name} (retry {retry.ToString()}/{RetryCount.ToString()}, waiting {ts.ToString()}): {ex.Message}");
                    return Task.CompletedTask;
                });

        /// <summary>
        /// Throttles the <see cref="GitHubAppsClient"/> when <see cref="AbuseException"/> or <see cref="RateLimitExceededException"/> is received.
        /// </summary>
        /// <remarks>
        /// Adapted from https://github.com/octokit/octokit.net/issues/1792#issue-311651300, see that issue for a more detailed discussion.
        /// </remarks>
        public static async Task<T> ThrottleAsync<T>(
            this GitHubClient client,
            Func<GitHubClient, Task<T>> operation,
            IExecutionContext context) =>
            await _retryPolicy.ExecuteAsync(
                async (ctx, _) => await operation((GitHubClient)ctx[nameof(GitHubClient)]),
                new Context
                {
                    { nameof(IExecutionContext), context },
                    { nameof(GitHubClient), client }
                },
                context.CancellationToken);

        /// <summary>
        /// Throttles the <see cref="GitHubAppsClient"/> when <see cref="AbuseException"/> or <see cref="RateLimitExceededException"/> is received.
        /// </summary>
        /// <remarks>
        /// Adapted from https://github.com/octokit/octokit.net/issues/1792#issue-311651300, see that issue for a more detailed discussion.
        /// </remarks>
        public static async Task ThrottleAsync(
            this GitHubClient client,
            Func<GitHubClient, Task> operation,
            IExecutionContext context) =>
            await _retryPolicy.ExecuteAsync(
                async (ctx, _) => await operation((GitHubClient)ctx[nameof(GitHubClient)]),
                new Context
                {
                    { nameof(IExecutionContext), context },
                    { nameof(GitHubClient), client }
                },
                context.CancellationToken);
    }
}