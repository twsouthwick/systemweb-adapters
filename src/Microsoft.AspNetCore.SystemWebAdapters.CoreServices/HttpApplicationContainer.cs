// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal partial class HttpApplicationContainer<TApp> : IHttpApplicationContainer, IDisposable
    where TApp : HttpApplication, new()
{
    [LoggerMessage(0, LogLevel.Information, "Registered event {ApplicationType}.{EventName}")]
    private static partial void LogRegistration(ILogger logger, string applicationType, string eventName);

    [LoggerMessage(1, LogLevel.Warning, "HttpApplication event {ApplicationType}.{EventName} is unsupported")]
    private static partial void LogUnsupported(ILogger logger, string applicationType, string eventName);

    [LoggerMessage(2, LogLevel.Warning, "{ApplicationType}.{EventName} has unsupported signature")]
    private static partial void LogInvalid(ILogger logger, string applicationType, string eventName);

    private readonly Lazy<HttpApplication> _application;

    public HttpApplicationContainer(IEnumerable<IHttpModule> modules, ILogger<HttpApplicationContainer<TApp>> logger)
    {
        _application = new(() =>
        {
            var moduleList = modules.ToList();

            var httpApp = new TApp();

            RegisterMethods(httpApp, logger);
            httpApp.Initialize(moduleList);

            return httpApp;
        }, isThreadSafe: true);
    }

    public HttpApplication Application => _application.Value;

    public void Dispose()
    {
        if (_application.IsValueCreated)
        {
            _application.Value.Dispose();
        }
    }

    private static void RegisterMethods(HttpApplication application, ILogger logger)
    {
        var known = HookupEvents(application, logger);

        known.Init?.Invoke(application, EventArgs.Empty);
        known.Start?.Invoke(application, EventArgs.Empty);
    }

    private static KnownEvents HookupEvents(HttpApplication application, ILogger logger)
    {
        var typeName = application.GetType().FullName ?? application.GetType().Name;
        var known = new KnownEvents();

        foreach (var method in application.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var state = State.None;

            switch (method.Name)
            {
                // Fired when an application initializes or is first called. It's invoked for all HttpApplication object instances.
                case "Application_Init":
                    known = known with { Init = CreateHandler(method, ref state) };
                    break;

                // Fired when the first instance of the HttpApplication class is created. It allows you to create objects that are accessible by all HttpApplication instances.
                case "Application_Start":
                    known = known with { Start = CreateHandler(method, ref state) };
                    break;

                // Fired when the last instance of an HttpApplication class is destroyed. It's fired only once during an application's lifetime.
                // We're only creating a single instance of the application so we can just call this at Disposed
                case "Application_End":
                    application.Disposed += CreateHandler(method, ref state);
                    break;

                // Fired just before an application is destroyed. This is the ideal location for cleaning up previously used resources.
                case "Application_Disposed":
                    application.Disposed += CreateHandler(method, ref state);
                    break;

                // Fired when an unhandled exception is encountered within the application.
                case "Application_Error":
                    application.Error += CreateHandler(method, ref state);
                    break;

                // Fired when an application request is received. It's the first event fired for a request, which is often a page request (URL) that a user enters.
                case "Application_BeginRequest":
                    application.BeginRequest += CreateHandler(method, ref state);
                    break;

                // The last event fired for an application request.
                case "Application_EndRequest":
                    application.EndRequest += CreateHandler(method, ref state);
                    break;

                // Fired before the ASP.NET page framework begins executing an event handler like a page or Web service.
                case "Application_PreRequestHandlerExecute":
                    application.PreRequestHandlerExecute += CreateHandler(method, ref state);
                    break;

                // Fired when the ASP.NET page framework is finished executing an event handler.
                case "Application_PostRequestHandlerExecute":
                    application.PostRequestHandlerExecute += CreateHandler(method, ref state);
                    break;

                // Fired before the ASP.NET page framework sends HTTP headers to a requesting client (browser).
                case "Application_PreSendRequestHeaders":
                    //application.PreSendRequestHeaders += CreateHandler(method, ref state);
                    state = State.NotSupported;
                    break;

                // Fired before the ASP.NET page framework sends content to a requesting client (browser).
                case "Application_PreSendContent":
                    //application.PreSendContent += CreateHandler(method, ref state);
                    state = State.NotSupported;
                    break;

                // Fired when the ASP.NET page framework gets the current state (Session state) related to the current request.
                case "Application_AcquireRequestState":
                    application.AcquireRequestState += CreateHandler(method, ref state);
                    break;

                // Fired when the ASP.NET page framework completes execution of all event handlers. This results in all state modules to save their current state data.
                case "Application_ReleaseRequestState":
                    application.ReleaseRequestState += CreateHandler(method, ref state);
                    break;

                // Fired when the ASP.NET page framework completes an authorization request. It allows caching modules to serve the request from the cache, thus bypassing handler execution.
                case "Application_ResolveRequestCache":
                    application.ResolveRequestCache += CreateHandler(method, ref state);
                    break;

                // Fired when the ASP.NET page framework completes handler execution to allow caching modules to store responses to be used to handle subsequent requests.
                case "Application_UpdateRequestCache":
                    application.UpdateRequestCache += CreateHandler(method, ref state);
                    break;

                // Fired when the security module has established the current user's identity as valid. At this point, the user's credentials have been validated.
                case "Application_AuthenticateRequest":
                    application.AuthenticateRequest += CreateHandler(method, ref state);
                    break;

                // Fired when the security module has verified that a user can access resources.
                case "Application_AuthorizeRequest":
                    application.AuthorizeRequest += CreateHandler(method, ref state);
                    break;

                // Fired when a new user visits the application Web site.
                case "Session_Start":
                    application.SessionStart = CreateHandler(method, ref state);
                    break;

                // Fired when a user's session times out, ends, or they leave the application Web site.
                case "Session_End":
                    application.SessionEnd = CreateHandler(method, ref state);
                    break;
            }

            if (state is State.Registered)
            {
                LogRegistration(logger, typeName, method.Name);
            }
            else if (state is State.NotSupported)
            {
                LogUnsupported(logger, typeName, method.Name);
            }
            else if (state is State.InvalidSignature)
            {
                LogInvalid(logger, typeName, method.Name);
            }
        }

        return known;

        EventHandler? CreateHandler(MethodInfo method, ref State state)
        {
            var parameters = method.GetParameters();

            if (method.ReturnType == typeof(void))
            {
                if (parameters.Length == 0)
                {
                    state = State.Registered;
                    var d = method.CreateDelegate<Action>(application);

                    return (s, e) => d();
                }

                if (parameters.Length == 2 && parameters[0].ParameterType == typeof(object) && parameters[1].ParameterType == typeof(EventArgs))
                {
                    state = State.Registered;
                    return method.CreateDelegate<EventHandler>(application);
                }
            }

            state = State.InvalidSignature;
            return null;
        }
    }

    private readonly record struct KnownEvents
    {
        public EventHandler? Init { get; init; }

        public EventHandler? Start { get; init; }
    }

    private enum State
    {
        None,
        Registered,
        NotSupported,
        InvalidSignature,
    }
}
