// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static class HttpApplicationEventsExtensions
{
    /// <summary>
    /// Adds the supplied <see cref="IHttpApplicationEventsFeature"/> to the feature collection or creates a composite feature to ensure the existing events are raised as well.
    /// </summary>
    internal static IHttpApplicationEventsFeature SetOrUpdateEventsFeature(this IFeatureCollection features, IHttpApplicationEventsFeature feature)
    {
        if (features.Get<IHttpApplicationEventsFeature>() is { } existing)
        {
            var composite = new CompositeHttpApplicationEventsFeature(existing, feature);
            features.Set<IHttpApplicationEventsFeature>(composite);
            return composite;
        }
        else
        {
            features.Set(feature);
            return feature;
        }
    }

    private class CompositeHttpApplicationEventsFeature : IHttpApplicationEventsFeature
    {
        private readonly IHttpApplicationEventsFeature _first;
        private readonly IHttpApplicationEventsFeature _second;

        public CompositeHttpApplicationEventsFeature(IHttpApplicationEventsFeature first, IHttpApplicationEventsFeature second)
        {
            _first = first;
            _second = second;
        }

        public void Dispose()
        {
            _first.Dispose();
            _second.Dispose();
        }

        public async ValueTask RaiseAcquireRequestStateAsync(CancellationToken token)
        {
            await _first.RaiseAcquireRequestStateAsync(token);
            await _second.RaiseAcquireRequestStateAsync(token);
        }

        public async ValueTask RaiseAuthenticateRequestAsync(CancellationToken token)
        {
            await _first.RaiseAuthenticateRequestAsync(token);
            await _second.RaiseAuthenticateRequestAsync(token);
        }

        public async ValueTask RaiseAuthorizeRequestAsync(CancellationToken token)
        {
            await _first.RaiseAuthorizeRequestAsync(token);
            await _second.RaiseAuthorizeRequestAsync(token);
        }

        public async ValueTask RaiseBeginRequestAsync(CancellationToken token)
        {
            await _first.RaiseBeginRequestAsync(token);
            await _second.RaiseBeginRequestAsync(token);
        }

        public async ValueTask RaiseEndRequestAsync(CancellationToken token)
        {
            await _first.RaiseEndRequestAsync(token);
            await _second.RaiseEndRequestAsync(token);
        }

        public async ValueTask RaiseErrorAsync(CancellationToken token)
        {
            await _first.RaiseErrorAsync(token);
            await _second.RaiseErrorAsync(token);
        }

        public async ValueTask RaiseLogRequestAsync(CancellationToken token)
        {
            await _first.RaiseLogRequestAsync(token);
            await _second.RaiseLogRequestAsync(token);
        }

        public async ValueTask RaiseMapRequestHandlerAsync(CancellationToken token)
        {
            await _first.RaiseMapRequestHandlerAsync(token);
            await _second.RaiseMapRequestHandlerAsync(token);
        }

        public async ValueTask RaisePostAcquireRequestStateAsync(CancellationToken token)
        {
            await _first.RaisePostAcquireRequestStateAsync(token);
            await _second.RaisePostAcquireRequestStateAsync(token);
        }

        public async ValueTask RaisePostAuthenticateRequestAsync(CancellationToken token)
        {
            await _first.RaisePostAuthenticateRequestAsync(token);
            await _second.RaisePostAuthenticateRequestAsync(token);
        }

        public async ValueTask RaisePostAuthorizeRequestAsync(CancellationToken token)
        {
            await _first.RaisePostAuthorizeRequestAsync(token);
            await _second.RaisePostAuthorizeRequestAsync(token);
        }

        public async ValueTask RaisePostLogRequestAsync(CancellationToken token)
        {
            await _first.RaisePostLogRequestAsync(token);
            await _second.RaisePostLogRequestAsync(token);
        }

        public async ValueTask RaisePostMapRequestHandlerAsync(CancellationToken token)
        {
            await _first.RaisePostMapRequestHandlerAsync(token);
            await _second.RaisePostMapRequestHandlerAsync(token);
        }

        public async ValueTask RaisePostReleaseRequestStateAsync(CancellationToken token)
        {
            await _first.RaisePostReleaseRequestStateAsync(token);
            await _second.RaisePostReleaseRequestStateAsync(token);
        }

        public async ValueTask RaisePostRequestHandlerExecuteAsync(CancellationToken token)
        {
            await _first.RaisePostRequestHandlerExecuteAsync(token);
            await _second.RaisePostRequestHandlerExecuteAsync(token);
        }

        public async ValueTask RaisePostResolveRequestCacheAsync(CancellationToken token)
        {
            await _first.RaisePostResolveRequestCacheAsync(token);
            await _second.RaisePostResolveRequestCacheAsync(token);
        }

        public async ValueTask RaisePostUpdateRequestCacheAsync(CancellationToken token)
        {
            await _first.RaisePostUpdateRequestCacheAsync(token);
            await _second.RaisePostUpdateRequestCacheAsync(token);
        }

        public async ValueTask RaisePreRequestHandlerExecuteAsync(CancellationToken token)
        {
            await _first.RaisePreRequestHandlerExecuteAsync(token);
            await _second.RaisePreRequestHandlerExecuteAsync(token);
        }

        public async ValueTask RaiseReleaseRequestStateAsync(CancellationToken token)
        {
            await _first.RaiseReleaseRequestStateAsync(token);
            await _second.RaiseReleaseRequestStateAsync(token);
        }

        public async ValueTask RaiseRequestCompletedAsync(CancellationToken token)
        {
            await _first.RaiseRequestCompletedAsync(token);
            await _second.RaiseRequestCompletedAsync(token);
        }

        public async ValueTask RaiseResolveRequestCacheAsync(CancellationToken token)
        {
            await _first.RaiseResolveRequestCacheAsync(token);
            await _second.RaiseResolveRequestCacheAsync(token);
        }

        public async ValueTask RaiseSessionEnd(CancellationToken token)
        {
            await _first.RaiseSessionEnd(token);
            await _second.RaiseSessionEnd(token);
        }

        public async ValueTask RaiseSessionStart(CancellationToken token)
        {
            await _first.RaiseSessionStart(token);
            await _second.RaiseSessionStart(token);
        }

        public async ValueTask RaiseUpdateRequestCacheAsync(CancellationToken token)
        {
            await _first.RaiseUpdateRequestCacheAsync(token);
            await _second.RaiseUpdateRequestCacheAsync(token);
        }
    }
}
