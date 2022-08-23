// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class HttpApplicationExtensions
{
    public static ISystemWebAdapterBuilder AddHttpModule<TModule>(this ISystemWebAdapterBuilder builder, Action<HttpApplicationOptions>? configure = null)
        where TModule : class, IHttpModule
    {
        builder.Services.TryAddSingleton<IHttpApplicationContainer, HttpApplicationContainer<HttpApplication>>();
        builder.Services.AddTransient<IHttpModule, TModule>();

        return builder.AddHttpApplicationInternal(configure);
    }

    public static ISystemWebAdapterBuilder AddHttpApplication<TApp>(this ISystemWebAdapterBuilder builder, Action<HttpApplicationOptions>? configure = null)
        where TApp : HttpApplication, new()
    {
        builder.Services.AddSingleton<IHttpApplicationContainer, HttpApplicationContainer<TApp>>();

        return builder.AddHttpApplicationInternal(configure);
    }

    private static ISystemWebAdapterBuilder AddHttpApplicationInternal(this ISystemWebAdapterBuilder builder, Action<HttpApplicationOptions>? configure)
    {
        var options = builder.Services.AddOptions<HttpApplicationOptions>()
            .Configure(options =>
            {
                options.SetCurrentNotification = true;
                options.UseAuthentication = true;
                options.UseAuthorization = true;
                options.EnableHandlers = true;
                options.EnableHttpApplication = true;
            });

        if (configure is not null)
        {
            options.Configure(configure);
        }

        return builder;
    }

    internal static void UseHttpApplicationAuth(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<HttpApplicationOptions>>().Value;

        if (options.UseAuthentication)
        {
            app.UseHttpApplicationAuthentication();
        }

        if (options.UseAuthorization)
        {
            app.UseHttpApplicationAuthorization();
        }
    }

    private static void UseHttpApplicationAuthentication(this IApplicationBuilder app)
    {
        // Check is valid for .NET 7+
        const string AuthenticationMiddlewareSetKey = "__AuthenticationMiddlewareSet";
        const string HttpApplicationAuthenticationMiddlewareSetKey = "__HttpApplicationAuthenticationMiddlewareSet";
        const string Message = "Authentication has already been added. If registering authentication to be aware of HttpApplication, remove other calls to IApplicationBuilder.UseAuthentication()";

        app.CheckExistingMiddleware(HttpApplicationAuthenticationMiddlewareSetKey, AuthenticationMiddlewareSetKey, Message);

        // Raise events even if the built-in authentication middleware is not run
        if (app.ApplicationServices.GetService<IAuthenticationSchemeProvider>() is not null)
        {
            app.UseAuthentication();
        }

        app.UseMiddleware<HttpApplicationEventsMiddleware.RaiseAuthenticateRequest>();
    }

    private static void UseHttpApplicationAuthorization(this IApplicationBuilder app)
    {
        const string AuthorizationMiddlewareSetKey = "__AuthorizationMiddlewareSet";
        const string HttpApplicationAuthorizationMiddlewareSetKey = "__HttpApplicationAuthorizationMiddlewareSet";
        const string Message = "Authorization has already been added. If registering authorization to be aware of HttpApplication, remove other calls to IApplicationBuilder.UseAuthorization()";

        app.CheckExistingMiddleware(HttpApplicationAuthorizationMiddlewareSetKey, AuthorizationMiddlewareSetKey, Message);


        if (app.ApplicationServices.GetService<IAuthorizationHandlerProvider>() is not null)
        {
            app.UseAuthorization();
        }

        // Raise events even if the built-in authorization middleware is not run
        app.UseMiddleware<HttpApplicationEventsMiddleware.RaiseAuthorizeRequest>();
    }

    private static void CheckExistingMiddleware(this IApplicationBuilder app, string httpApplicationMiddleware, string frameworkMiddleware, string message)
    {
        if (app.Properties.ContainsKey(httpApplicationMiddleware))
        {
            return;
        }

        app.Properties[httpApplicationMiddleware] = true;

        // Check is valid for .NET 7+
        if (app.Properties.ContainsKey(frameworkMiddleware))
        {
            throw new InvalidOperationException(message);
        }
    }
}
