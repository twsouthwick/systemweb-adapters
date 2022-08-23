// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class SystemWebAdaptersAppBuilderTests
{
    private static readonly ImmutableList<string> AllOrderedExpectedEvents = new[]
    {
        nameof(IHttpApplicationEventsFeature.RaiseBeginRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaiseAuthenticateRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostAuthenticateRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaiseAuthorizeRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostAuthorizeRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaiseResolveRequestCacheAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostResolveRequestCacheAsync),
        nameof(IHttpApplicationEventsFeature.RaiseMapRequestHandlerAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostMapRequestHandlerAsync),
        nameof(IHttpApplicationEventsFeature.RaiseSessionStart),
        nameof(IHttpApplicationEventsFeature.RaiseAcquireRequestStateAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostAcquireRequestStateAsync),
        nameof(IHttpApplicationEventsFeature.RaisePreRequestHandlerExecuteAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostRequestHandlerExecuteAsync),
        nameof(IHttpApplicationEventsFeature.RaiseSessionEnd),
        nameof(IHttpApplicationEventsFeature.RaiseReleaseRequestStateAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostReleaseRequestStateAsync),
        nameof(IHttpApplicationEventsFeature.RaiseUpdateRequestCacheAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostUpdateRequestCacheAsync),
        nameof(IHttpApplicationEventsFeature.RaiseLogRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostLogRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaiseEndRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaiseRequestCompletedAsync),
    }.ToImmutableList();

    private static readonly ImmutableList<string> AuthenticationEvents = new[]
    {
        nameof(IHttpApplicationEventsFeature.RaiseAuthenticateRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostAuthenticateRequestAsync),
    }.ToImmutableList();

    private static readonly ImmutableList<string> AuthorizationEvents = new[]
    {
        nameof(IHttpApplicationEventsFeature.RaiseAuthorizeRequestAsync),
        nameof(IHttpApplicationEventsFeature.RaisePostAuthorizeRequestAsync),
    }.ToImmutableList();

    [InlineData(true, true, SessionState.None)]
    [InlineData(true, true, SessionState.New)]
    [InlineData(true, true, SessionState.Exisiting)]
    [InlineData(true, true, SessionState.Abandoned)]
    [InlineData(true, false, SessionState.None)]
    [InlineData(false, true, SessionState.None)]
    [InlineData(false, false, SessionState.None)]
    [Theory]
    public async Task RaiseAllEvents(bool useAuthentication, bool useAuthorization, SessionState state)
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var events = new Mock<IHttpApplicationEventsFeature>();
        var services = new ServiceCollection();
        var expected = AllOrderedExpectedEvents;

        services.AddLogging();
        services.AddSystemWebAdapters();
        services.AddOptions<HttpApplicationOptions>()
            .Configure(options =>
            {
                options.UseAuthentication = useAuthentication;
                options.UseAuthorization = useAuthorization;
            });

        var session = new Mock<ISessionState>();

        if (state is SessionState.New)
        {
            session.Setup(s => s.IsNewSession).Returns(true);
        }
        else
        {
            expected = expected.Remove(nameof(IHttpApplicationEventsFeature.RaiseSessionStart));
        }

        if (state is SessionState.Abandoned)
        {
            session.Setup(s => s.IsAbandoned).Returns(true);
        }
        else
        {
            expected = expected.Remove(nameof(IHttpApplicationEventsFeature.RaiseSessionEnd));
        }

        if (state is not SessionState.None)
        {
            var attribute = new SessionAttribute();
            var sessionManager = new Mock<ISessionManager>();

            sessionManager.Setup(s => s.CreateAsync(httpContext, attribute)).ReturnsAsync(session.Object);

            services.AddSingleton(sessionManager.Object);
            httpContext.SetEndpoint(new Endpoint(null, new EndpointMetadataCollection(attribute), null));
        }

        using var provider = services.BuildServiceProvider();

        httpContext.RequestServices = provider;

        var pipeline = CreatePipeline(provider, app => app.UseSystemWebAdapters());

        httpContext.Features.Set(events.Object);

        if (!useAuthentication)
        {
            expected = expected.RemoveRange(AuthenticationEvents);
        }

        if (!useAuthorization)
        {
            expected = expected.RemoveRange(AuthorizationEvents);
        }

        // Act
        await pipeline(httpContext);

        // Assert
        var names = events.Invocations.Select(i => i.Method.Name).ToImmutableList();
        Assert.Equal(expected.Count, names.Count);
        Assert.Equal(expected, names);
    }

    [Fact]
    public void TestPipeline()
    {
        // Arrange
        var expected = new[]
        {
            typeof(SetHttpContextTimestampMiddleware).FullName,
            typeof(SetNotificationMiddleware).FullName,
            typeof(SetHttpApplicationMiddleware).FullName,
            typeof(SetHttpHandlerMiddleware).FullName,
            typeof(HttpApplicationEventsMiddleware.BeginEnd).FullName,
            typeof(HttpApplicationEventsMiddleware.RaiseAuthenticateRequest).FullName,
            typeof(HttpApplicationEventsMiddleware.RaiseAuthorizeRequest).FullName,
            typeof(HttpApplicationMiddleEventsMiddleware).FullName,
            typeof(SessionMiddleware).FullName,
            typeof(DefaultCacheControlMiddleware).FullName,
            typeof(PreBufferRequestStreamMiddleware).FullName,
            typeof(BufferResponseStreamMiddleware).FullName,
            typeof(SingleThreadedRequestMiddleware).FullName,
            typeof(CurrentPrincipalMiddleware).FullName,
            typeof(HttpApplicationEventsHandlerMiddleware).FullName,
            "Microsoft.AspNetCore.Builder.ApplicationBuilder+<>c",
        };

        var httpContext = new DefaultHttpContext();
        var events = new Mock<IHttpApplicationEventsFeature>();
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddOptions();
        services.AddSystemWebAdapters();
        services.AddOptions<HttpApplicationOptions>()
            .Configure(options =>
            {
                options.UseAuthentication = true;
                options.UseAuthorization = true;
                options.EnableHttpApplication = true;
                options.EnableHandlers = true;
                options.SetCurrentNotification = true;
            });

        using var provider = services.BuildServiceProvider();

        var pipeline = CreatePipeline(provider, builder => builder.UseSystemWebAdapters());

        // Act
        var targets = GetPipelineTargets(pipeline).ToArray();

        // Assert
        Assert.Equal(expected, targets);
    }

    public enum SessionState
    {
        None,
        New,
        Exisiting,
        Abandoned
    }

    private static RequestDelegate CreatePipeline(IServiceProvider provider, Action<IApplicationBuilder> configure)
    {
        var startupFilters = provider.GetService<IEnumerable<IStartupFilter>>();

        if (startupFilters is not null)
        {
            foreach (var filter in startupFilters.Reverse())
            {
                configure = filter.Configure(configure);
            }
        }

        var builder = new ApplicationBuilder(provider);
        configure(builder);
        return builder.Build();
    }

    private static IEnumerable<string> GetPipelineTargets(RequestDelegate del)
    {
        Delegate? d = del;

        while (d is { Target: { } target })
        {
            yield return target.ToString() ?? "--Unknown--";

            d = GetNext(target.GetType()) is { } field && field.GetValue(target) is Delegate next ? next : null;
        }

        // Look for any field at any level that is of type RequestDelegate
        static FieldInfo? GetNext(Type type)
        {
            var t = type;

            while (t is not null)
            {
                var field = t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(f => f.FieldType == typeof(RequestDelegate));

                if (field is not null)
                {
                    return field;
                }

                t = t.BaseType;
            }

            return null;
        }
    }
}
