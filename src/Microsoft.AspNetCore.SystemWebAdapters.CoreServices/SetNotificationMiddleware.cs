// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class SetNotificationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptions<HttpApplicationOptions> _options;

    public SetNotificationMiddleware(RequestDelegate next, IOptions<HttpApplicationOptions> options)
    {
        _next = next;
        _options = options;
    }

    public Task InvokeAsync(HttpContextCore context)
    {
        if (_options.Value.SetCurrentNotification)
        {
            var currentNotification = new SetCurrentNotificationEventsFeature();

            context.Features.SetOrUpdateEventsFeature(currentNotification);
            context.Features.Set<INotificationFeature>(currentNotification);
        }

        return _next(context);
    }

    private class SetCurrentNotificationEventsFeature : IHttpApplicationEventsFeature, INotificationFeature
    {
        public RequestNotification CurrentNotification { get; set; }

        public bool IsPostNotification { get; set; }

        public void Dispose()
        {
        }

        public ValueTask RaiseAcquireRequestStateAsync(CancellationToken token)
            => SetNotification(RequestNotification.AcquireRequestState);

        public ValueTask RaiseAuthenticateRequestAsync(CancellationToken token)
            => SetNotification(RequestNotification.AuthenticateRequest);

        public ValueTask RaiseAuthorizeRequestAsync(CancellationToken token)
            => SetNotification(RequestNotification.AuthorizeRequest);

        public ValueTask RaiseBeginRequestAsync(CancellationToken token)
            => SetNotification(RequestNotification.BeginRequest);

        public ValueTask RaiseEndRequestAsync(CancellationToken token)
            => SetNotification(RequestNotification.EndRequest);

        public ValueTask RaiseErrorAsync(CancellationToken token) => ValueTask.CompletedTask;

        public ValueTask RaiseLogRequestAsync(CancellationToken token)
            => SetNotification(RequestNotification.LogRequest);

        public ValueTask RaiseMapRequestHandlerAsync(CancellationToken token)
            => SetNotification(RequestNotification.MapRequestHandler);

        public ValueTask RaisePostAcquireRequestStateAsync(CancellationToken token)
            => SetPostNotification(RequestNotification.AcquireRequestState);

        public ValueTask RaisePostAuthenticateRequestAsync(CancellationToken token) 
            => SetPostNotification(RequestNotification.AuthenticateRequest);

        public ValueTask RaisePostAuthorizeRequestAsync(CancellationToken token) 
            => SetPostNotification(RequestNotification.AuthorizeRequest);

        public ValueTask RaisePostLogRequestAsync(CancellationToken token) 
            => SetPostNotification(RequestNotification.LogRequest);

        public ValueTask RaisePostMapRequestHandlerAsync(CancellationToken token) 
            => SetPostNotification(RequestNotification.MapRequestHandler);

        public ValueTask RaisePostReleaseRequestStateAsync(CancellationToken token) 
            => SetPostNotification(RequestNotification.ReleaseRequestState);

        public ValueTask RaisePostRequestHandlerExecuteAsync(CancellationToken token) 
            => SetPostNotification(RequestNotification.ExecuteRequestHandler);

        public ValueTask RaisePostResolveRequestCacheAsync(CancellationToken token) 
            => SetPostNotification(RequestNotification.ResolveRequestCache);

        public ValueTask RaisePostUpdateRequestCacheAsync(CancellationToken token) 
            => SetPostNotification(RequestNotification.UpdateRequestCache);

        public ValueTask RaisePreRequestHandlerExecuteAsync(CancellationToken token)
            => SetNotification(RequestNotification.PreExecuteRequestHandler);

        public ValueTask RaiseReleaseRequestStateAsync(CancellationToken token)
            => SetNotification(RequestNotification.ReleaseRequestState);

        public ValueTask RaiseRequestCompletedAsync(CancellationToken token)
            => SetNotification(RequestNotification.EndRequest);

        public ValueTask RaiseResolveRequestCacheAsync(CancellationToken token)
            => SetNotification(RequestNotification.ResolveRequestCache);

        public ValueTask RaiseSessionEnd(CancellationToken token) => ValueTask.CompletedTask;

        public ValueTask RaiseSessionStart(CancellationToken token) => ValueTask.CompletedTask;

        public ValueTask RaiseUpdateRequestCacheAsync(CancellationToken token)
            => SetNotification(RequestNotification.UpdateRequestCache);

        private ValueTask SetPostNotification(RequestNotification notification)
            => SetNotification(notification, true);

        private ValueTask SetNotification(RequestNotification notification, bool isPost = false)
        {
            CurrentNotification = notification;
            IsPostNotification = isPost;
            return ValueTask.CompletedTask;
        }
    }
}
