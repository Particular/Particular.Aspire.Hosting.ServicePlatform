namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using System.Collections.Concurrent;
using System.Collections.Generic;

sealed class PlatformReadinessState
{
#pragma warning disable PS0025 // Dictionary keys should implement IEquatable<T>
    readonly ConcurrentDictionary<ParticularPlatformResource, Tracker> trackers = new();
#pragma warning restore PS0025 // Dictionary keys should implement IEquatable<T>

    public void Register(ParticularPlatformResource platform, int expectedChildCount) => trackers[platform] = new Tracker(expectedChildCount);
    public bool MarkReady(ParticularPlatformResource platform, string resourceName) =>
        trackers.TryGetValue(platform, out var tracker) &&
        tracker.ExpectedCount > 0 &&
        tracker.MarkReady(resourceName);

    public bool MarkStopped(ParticularPlatformResource platform, string resourceName) =>
        trackers.TryGetValue(platform, out var tracker) &&
        tracker.MarkStopped(resourceName);

    sealed class Tracker(int expectedCount)
    {
        readonly HashSet<string> readyResources = [];
        readonly object readyLock = new();
        bool published;

        public int ExpectedCount { get; } = expectedCount;

        public bool MarkReady(string resourceName)
        {
            lock (readyLock)
            {
                readyResources.Add(resourceName);
                if (published || readyResources.Count < ExpectedCount)
                {
                    return false;
                }
                published = true;
                return true;
            }
        }

        public bool MarkStopped(string resourceName)
        {
            lock (readyLock)
            {
                var removed = readyResources.Remove(resourceName);
                if (removed)
                {
                    published = false;
                }
                return removed;
            }
        }
    }
}