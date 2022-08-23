// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web.Caching;
using System.Web.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class SystemWebAdaptersExtensions
{
    public static ISystemWebAdapterBuilder AddSystemWebAdapters(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<Cache>();
        services.AddSingleton<BrowserCapabilitiesFactory>();
        services.AddTransient<IStartupFilter, SystemWebAdaptersStartupFilter>();

        return new SystemWebAdapterBuilder(services)
            .AddMvc();
    }

    public static void UseSystemWebAdapters(this IApplicationBuilder app)
    {
        app.UseHttpApplicationAuth();

        app.UseMiddleware<HttpApplicationMiddleEventsMiddleware>();
        app.UseMiddleware<SessionMiddleware>();
        app.UseMiddleware<DefaultCacheControlMiddleware>();
        app.UseMiddleware<PreBufferRequestStreamMiddleware>();
        app.UseMiddleware<BufferResponseStreamMiddleware>();
        app.UseMiddleware<SingleThreadedRequestMiddleware>();
        app.UseMiddleware<CurrentPrincipalMiddleware>();
        app.UseMiddleware<HttpApplicationEventsHandlerMiddleware>();
    }

    /// <summary>
    /// Adds request stream buffering to the endpoint(s)
    /// </summary>
    public static TBuilder PreBufferRequestStream<TBuilder>(this TBuilder builder, PreBufferRequestStreamAttribute? metadata = null)
        where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(metadata ?? new PreBufferRequestStreamAttribute());

    /// <summary>
    /// Adds session support for System.Web adapters for the endpoint(s)
    /// </summary>
    public static TBuilder RequireSystemWebAdapterSession<TBuilder>(this TBuilder builder, SessionAttribute? metadata = null)
        where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(metadata ?? new SessionAttribute());

    /// <summary>
    /// Ensure response stream is buffered to enable synchronous actions on it for the endpoint(s)
    /// </summary>
    public static TBuilder BufferResponseStream<TBuilder>(this TBuilder builder, BufferResponseStreamAttribute? metadata = null)
        where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(metadata ?? new BufferResponseStreamAttribute());

    internal class SystemWebAdaptersStartupFilter : IStartupFilter
    {
        private readonly IOptions<HttpApplicationOptions> _options;

        public SystemWebAdaptersStartupFilter(IOptions<HttpApplicationOptions> options)
        {
            _options = options;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                var options = _options.Value;
                
                builder.UseMiddleware<SetHttpContextTimestampMiddleware>();

                if (options.SetCurrentNotification)
                {
                    builder.UseMiddleware<SetNotificationMiddleware>();
                }

                if (options.EnableHttpApplication)
                {
                    builder.UseMiddleware<SetHttpApplicationMiddleware>();
                }

                if (options.EnableHandlers)
                {
                    builder.UseMiddleware<SetHttpHandlerMiddleware>();
                }

                builder.UseMiddleware<HttpApplicationEventsMiddleware.BeginEnd>();

                next(builder);
            };
    }
}
