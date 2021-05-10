using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using Statiq.Common;

namespace Statiq.Web.GitHub
{
    public static class GitHubClientExtensions
    {
        /// <summary>
        /// Throttles the <see cref="GitHubAppsClient"/> when <see cref="AbuseException"/> or <see cref="RateLimitExceededException"/> is received.
        /// </summary>
        /// <remarks>
        /// Adapted from https://github.com/octokit/octokit.net/issues/1792#issue-311651300, see that issue for a more detailed discussion.
        /// </remarks>
        public static async Task<T> ThrottleAsync<T>(
            this GitHubClient client,
            Func<GitHubClient, Task<T>> operation,
            CancellationToken cancellationToken = default,
            bool throwIfRateLimitExceeded = false)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return await operation(client).ConfigureAwait(false);
                }
                catch (AbuseException abuseException)
                {
                    await Task.Delay(TimeSpan.FromSeconds(abuseException.RetryAfterSeconds ?? 30), cancellationToken).ConfigureAwait(false);
                }
                catch (RateLimitExceededException limitException) when (!throwIfRateLimitExceeded)
                {
                    await Task.Delay(limitException.Reset - DateTimeOffset.Now, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Throttles the <see cref="GitHubAppsClient"/> when <see cref="AbuseException"/> or <see cref="RateLimitExceededException"/> is received.
        /// </summary>
        /// <remarks>
        /// Adapted from https://github.com/octokit/octokit.net/issues/1792#issue-311651300, see that issue for a more detailed discussion.
        /// </remarks>
        public static async Task ThrottleAsync(
            this GitHubClient client,
            Func<GitHubClient, Task> operation,
            CancellationToken cancellationToken = default,
            bool throwIfRateLimitExceeded = false)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await operation(client).ConfigureAwait(false);
                    return;
                }
                catch (AbuseException abuseException)
                {
                    await Task.Delay(TimeSpan.FromSeconds(abuseException.RetryAfterSeconds ?? 30), cancellationToken).ConfigureAwait(false);
                }
                catch (RateLimitExceededException limitException) when (!throwIfRateLimitExceeded)
                {
                    await Task.Delay(limitException.Reset - DateTimeOffset.Now, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
