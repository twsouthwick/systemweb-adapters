// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal abstract class HttpApplicationEventsMiddleware
{
    private readonly RequestDelegate _next;

    public HttpApplicationEventsMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
        => context.Features.Get<IHttpApplicationEventsFeature>() is { } events
            ? InvokeEventsAsync(_next, context, events)
            : _next(context);

    protected abstract Task InvokeEventsAsync(RequestDelegate next, HttpContext context, IHttpApplicationEventsFeature events);

    public class BeginEnd : HttpApplicationEventsMiddleware
    {
        public BeginEnd(RequestDelegate next)
            : base(next)
        {
        }

        protected override async Task InvokeEventsAsync(RequestDelegate next, HttpContext context, IHttpApplicationEventsFeature events)
        {
            await events.RaiseBeginRequestAsync(context.RequestAborted);
            await next(context);
            await events.RaiseEndRequestAsync(context.RequestAborted);
            await events.RaiseRequestCompletedAsync(context.RequestAborted);
        }
    }

    public class RaiseAuthenticateRequest : HttpApplicationEventsMiddleware
    {
        public RaiseAuthenticateRequest(RequestDelegate next)
            : base(next)
        {
        }

        protected override async Task InvokeEventsAsync(RequestDelegate next, HttpContext context, IHttpApplicationEventsFeature events)
        {
            await events.RaiseAuthenticateRequestAsync(context.RequestAborted);
            await events.RaisePostAuthenticateRequestAsync(context.RequestAborted);
            await next(context);
        }
    }

    public class RaiseAuthorizeRequest : HttpApplicationEventsMiddleware
    {
        public RaiseAuthorizeRequest(RequestDelegate next)
            : base(next)
        {
        }

        protected override async Task InvokeEventsAsync(RequestDelegate next, HttpContext context, IHttpApplicationEventsFeature events)
        {
            await events.RaiseAuthorizeRequestAsync(context.RequestAborted);
            await events.RaisePostAuthorizeRequestAsync(context.RequestAborted);
            await next(context);
        }
    }
}
