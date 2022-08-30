// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.Http;
#endif

#if NETSTANDARD2_0
#pragma warning disable CS0618 // Type or member is obsolete
#endif

namespace System.Web;

public static class HttpResponseAsyncExtensions
{
    private static Task CompletedTask
#if NET45
        { get; } = Task.FromResult(0);
#else
        => Task.CompletedTask;
#endif

    public static Task WriteFileAsync(this HttpResponse response, string filename, CancellationToken token)
#if NET6_0_OR_GREATER
        => response.TransmitFileAsync(filename, token);
#else
    {
        response.WriteFile(filename);
        return CompletedTask;
    }
#endif

    public static Task TransmitFileAsync(this HttpResponse response, string filename, long offset, long length, CancellationToken token)
#if NET6_0_OR_GREATER
        => response.UnwrapAdapter().SendFileAsync(filename, offset, length >= 0 ? length : null, token);
#else
    {
        response.TransmitFile(filename, offset, length);
        return CompletedTask;
    }
#endif

    public static Task EndAsync(this HttpResponse response)
#if NET6_0_OR_GREATER
        => response.AdapterFeature.EndAsync();
#else
    {
        response.End();
        return CompletedTask;
    }
#endif

    public static Task TransmitFileAsync(this HttpResponse response, string filename, CancellationToken token)
        => response.TransmitFileAsync(filename, 0, -1, token);

    public static Task RedirectPermanentAsync(this HttpResponse response, string url) => response.RedirectPermanentAsync(url, true);

    [SuppressMessage("Design", "CA1054:URI parameters should not be strings", Justification = "_writer is registered to be disposed by the owning HttpContext")]
#if NET6_0_OR_GREATER
    public static async Task RedirectPermanentAsync(this HttpResponse response, string url, bool endResponse)
    {
        response.UnwrapAdapter().Redirect(url, true);

        if (endResponse)
        {
            await response.AdapterFeature.EndAsync();
        }
    }
#else
    public static Task RedirectPermanentAsync(this HttpResponse response, string url, bool endResponse)
    {
        response.RedirectPermanent(url, endResponse);
        return CompletedTask;
    }
#endif
}
