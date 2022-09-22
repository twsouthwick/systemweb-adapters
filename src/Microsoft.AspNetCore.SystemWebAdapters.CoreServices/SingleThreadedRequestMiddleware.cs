// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class SingleThreadedRequestMiddleware
{
    private readonly RequestDelegate _next;

    public SingleThreadedRequestMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContextCore context)
        => context.GetEndpoint()?.Metadata.GetMetadata<SingleThreadedRequestAttribute>() is { IsDisabled: false }
            ? EnsureSingleThreaded(context)
            : _next(context);

    private Task EnsureSingleThreaded(HttpContextCore context)
    {
        var previousContext = SynchronizationContext.Current;
        var sync = new SingleThreadSynchronizationContext();

        SynchronizationContext.SetSynchronizationContext(sync);

        try
        {
            var task = _next(context).ContinueWith(t => sync.Complete(), TaskScheduler.FromCurrentSynchronizationContext());

            sync.RunOnCurrentThread(context.RequestAborted);

            return task;
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }

    private sealed class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<Message> _queue;
        private readonly int _threadId;

        public SingleThreadSynchronizationContext()
        {
            _threadId = Environment.CurrentManagedThreadId;
            _queue = new();
        }

        public override void Post(SendOrPostCallback d, object? state)
            => _queue.Add(new(d, state));

        public override void Send(SendOrPostCallback d, object? state)
        {
            if (_threadId == Environment.CurrentManagedThreadId)
            {
                try
                {
                    d(state);
                }
                catch (Exception ex)
                {
                    throw new TargetInvocationException(ex);
                }
            }
            else if (SendAndWait(d, state) is { } ex)
            {
                throw new TargetInvocationException(ex);
            }
        }

        private Exception? SendAndWait(SendOrPostCallback d, object? state)
        {
            Exception? exception = null;
            using var evt = new ManualResetEventSlim();

            void QueuedCallback(object? state)
            {
#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    d(state);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    evt.Set();
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            _queue.Add(new(QueuedCallback, state));

            evt.Wait();

            return exception;
        }

        public void RunOnCurrentThread(CancellationToken token)
        {
            while (_queue.TryTake(out var workItem, Timeout.Infinite, token))
            {
                if (workItem.Context is { } context)
                {
                    ExecutionContext.Run(context, new ContextCallback(workItem.Callback), workItem.State);
                }
                else
                {
                    workItem.Callback(workItem.State);
                }
            }
        }

        public void Complete() => _queue.CompleteAdding();

        private readonly record struct Message(SendOrPostCallback Callback, object? State)
        {
            public ExecutionContext? Context { get; } = ExecutionContext.Capture();
        }
    }
}
