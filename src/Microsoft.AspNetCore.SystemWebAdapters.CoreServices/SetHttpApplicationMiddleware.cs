// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class SetHttpApplicationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpApplicationContainer? _app;

    public SetHttpApplicationMiddleware(RequestDelegate next, IServiceProvider provider)
    {
        _next = next;
        _app = provider.GetService<IHttpApplicationContainer>();
    }

    public Task InvokeAsync(HttpContextCore context)
    {
        if (_app is not null)
        {
            context.Features.SetOrUpdateEventsFeature(_app.Application);
        }

        return _next(context);
    }
}
