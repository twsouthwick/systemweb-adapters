// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web;

public class HttpApplication : IDisposable, IHttpApplicationEventsFeature
{
    private List<IHttpModule>? _modules;

    public HttpApplication()
    {
    }

    internal void Initialize(List<IHttpModule> modules)
    {
        _modules = modules;

        foreach (var m in _modules)
        {
            m.Init(this);
        }
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = Constants.ApiFromAspNet)]
    public HttpContext? Context => HttpContext.Current;

    public void CompleteRequest() => Context?.Response.End();

    public event EventHandler? BeginRequest;

    ValueTask IHttpApplicationEventsFeature.RaiseBeginRequestAsync(CancellationToken token)
    {
        BeginRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? AuthenticateRequest;

    ValueTask IHttpApplicationEventsFeature.RaiseAuthenticateRequestAsync(CancellationToken token)
    {
        AuthenticateRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostAuthenticateRequest;

    ValueTask IHttpApplicationEventsFeature.RaisePostAuthenticateRequestAsync(CancellationToken token)
    {
        PostAuthenticateRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? AuthorizeRequest;

    ValueTask IHttpApplicationEventsFeature.RaiseAuthorizeRequestAsync(CancellationToken token)
    {
        AuthorizeRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostAuthorizeRequest;

    ValueTask IHttpApplicationEventsFeature.RaisePostAuthorizeRequestAsync(CancellationToken token)
    {
        PostAuthorizeRequest?.Invoke(this, EventArgs.Empty);

        return ValueTask.CompletedTask;
    }

    public event EventHandler? ResolveRequestCache;

    ValueTask IHttpApplicationEventsFeature.RaiseResolveRequestCacheAsync(CancellationToken token)
    {
        ResolveRequestCache?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostResolveRequestCache;

    ValueTask IHttpApplicationEventsFeature.RaisePostResolveRequestCacheAsync(CancellationToken token)
    {
        PostResolveRequestCache?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? MapRequestHandler;

    ValueTask IHttpApplicationEventsFeature.RaiseMapRequestHandlerAsync(CancellationToken token)
    {
        MapRequestHandler?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostMapRequestHandler;

    ValueTask IHttpApplicationEventsFeature.RaisePostMapRequestHandlerAsync(CancellationToken token)
    {
        PostMapRequestHandler?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? AcquireRequestState;

    ValueTask IHttpApplicationEventsFeature.RaiseAcquireRequestStateAsync(CancellationToken token)
    {
        AcquireRequestState?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostAcquireRequestState;

    ValueTask IHttpApplicationEventsFeature.RaisePostAcquireRequestStateAsync(CancellationToken token)
    {
        PostAcquireRequestState?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PreRequestHandlerExecute;

    ValueTask IHttpApplicationEventsFeature.RaisePreRequestHandlerExecuteAsync(CancellationToken token)
    {
        PreRequestHandlerExecute?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostRequestHandlerExecute;

    ValueTask IHttpApplicationEventsFeature.RaisePostRequestHandlerExecuteAsync(CancellationToken token)
    {
        PostRequestHandlerExecute?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? ReleaseRequestState;

    ValueTask IHttpApplicationEventsFeature.RaiseReleaseRequestStateAsync(CancellationToken token)
    {
        ReleaseRequestState?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostReleaseRequestState;

    ValueTask IHttpApplicationEventsFeature.RaisePostReleaseRequestStateAsync(CancellationToken token)
    {
        PostReleaseRequestState?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? UpdateRequestCache;

    ValueTask IHttpApplicationEventsFeature.RaiseUpdateRequestCacheAsync(CancellationToken token)
    {
        UpdateRequestCache?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostUpdateRequestCache;

    ValueTask IHttpApplicationEventsFeature.RaisePostUpdateRequestCacheAsync(CancellationToken token)
    {
        PostUpdateRequestCache?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? LogRequest;

    ValueTask IHttpApplicationEventsFeature.RaiseLogRequestAsync(CancellationToken token)
    {
        LogRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? PostLogRequest;

    ValueTask IHttpApplicationEventsFeature.RaisePostLogRequestAsync(CancellationToken token)
    {
        PostLogRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? EndRequest;

    ValueTask IHttpApplicationEventsFeature.RaiseEndRequestAsync(CancellationToken token)
    {
        EndRequest?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public event EventHandler? Error;

    ValueTask IHttpApplicationEventsFeature.RaiseErrorAsync(CancellationToken token)
    {
        Error?.Invoke(this, EventArgs.Empty);

        return ValueTask.CompletedTask;
    }

    public event EventHandler? RequestCompleted;

    ValueTask IHttpApplicationEventsFeature.RaiseRequestCompletedAsync(CancellationToken token)
    {
        RequestCompleted?.Invoke(this, EventArgs.Empty);

        return ValueTask.CompletedTask;
    }

    public event EventHandler? Disposed;

    public void Dispose()
    {
        Disposed?.Invoke(this, EventArgs.Empty);

        if (_modules is { } modules)
        {
            foreach (var module in modules)
            {
                module.Dispose();
            }
        }
    }

    internal EventHandler? SessionStart { get; set; }

    ValueTask IHttpApplicationEventsFeature.RaiseSessionStart(CancellationToken token)
    {
        SessionStart?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    internal EventHandler? SessionEnd { get; set; }

    ValueTask IHttpApplicationEventsFeature.RaiseSessionEnd(CancellationToken token)
    {
        SessionEnd?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }
}
