// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.CompilerServices;

#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.Http;
#endif

#if NETSTANDARD2_0
#pragma warning disable CS0618 // Type or member is obsolete
#endif

namespace System.Web;

public static class HttpRequestAsyncExtensions
{
    public static Task<Stream> GetInputStreamAsync(this HttpRequest request, CancellationToken token)
#if NET6_0_OR_GREATER
        => request.RequestFeature.GetInputStreamAsync(token);
#else
        => Task.FromResult(request.InputStream);
#endif

    public static Task<int> GetTotalBytesAsync(this HttpRequest request, CancellationToken token)
#if NET6_0_OR_GREATER
        => request.GetTotalBytesInternalAsync(token);
#else
        => Task.FromResult(request.TotalBytes);
#endif

    public static Task<byte[]> BinaryReadAsync(this HttpRequest request, int count, CancellationToken token)
#if NET6_0_OR_GREATER
        => request.BinaryReadInternalAsync(count, token);
#else
        => Task.FromResult(request.BinaryRead(count));
#endif

#if NET6_0_OR_GREATER
    private static async Task<int> GetTotalBytesInternalAsync(this HttpRequest request, CancellationToken token)
    {
        var stream = await request.GetInputStreamAsync(token);

        return (int)stream.Length;
    }

    internal static async Task<byte[]> BinaryReadInternalAsync(this HttpRequest request, int count, CancellationToken token)
    {
        var stream = await request.GetInputStreamAsync(token);

        return stream.BinaryRead(count);
    }

    internal static byte[] BinaryRead(this Stream stream, int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (count == 0)
        {
            return Array.Empty<byte>();
        }

        var buffer = new byte[count];
        var read = stream.Read(buffer);

        if (read == 0)
        {
            return Array.Empty<byte>();
        }

        if (read < count)
        {
            Array.Resize(ref buffer, read);
        }

        return buffer;
    }
#endif
}
