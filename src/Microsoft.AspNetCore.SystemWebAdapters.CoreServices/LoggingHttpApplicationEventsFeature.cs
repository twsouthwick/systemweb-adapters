// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal partial class LoggingHttpApplicationEventsFeature : IHttpApplicationEventsFeature
{
    [LoggerMessage(0, LogLevel.Trace, "Event IHttpApplicationEventsFeature.{Name} raised")]
    private partial void LogEvent([System.Runtime.CompilerServices.CallerMemberName] string? name = null);

    private readonly ILogger<LoggingHttpApplicationEventsFeature> _logger;

    public LoggingHttpApplicationEventsFeature(ILogger<LoggingHttpApplicationEventsFeature> logger)
    {
        _logger = logger;
    }

    public void Dispose()
    {
        LogEvent();
    }

    public ValueTask RaiseAcquireRequestStateAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaiseAuthenticateRequestAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaiseAuthorizeRequestAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaiseBeginRequestAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaiseEndRequestAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaiseErrorAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaiseLogRequestAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaiseMapRequestHandlerAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaisePostAcquireRequestStateAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaisePostAuthenticateRequestAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaisePostAuthorizeRequestAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaisePostLogRequestAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaisePostMapRequestHandlerAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaisePostReleaseRequestStateAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaisePostRequestHandlerExecuteAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaisePostResolveRequestCacheAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaisePostUpdateRequestCacheAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaisePreRequestHandlerExecuteAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaiseReleaseRequestStateAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaiseRequestCompletedAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaiseResolveRequestCacheAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaiseSessionEnd(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaiseSessionStart(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }

    public ValueTask RaiseUpdateRequestCacheAsync(CancellationToken token)
    {
        LogEvent();
        return ValueTask.CompletedTask;
    }
}
