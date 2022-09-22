using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests.Adapters;

public class SingleThreadedRequestMiddlewareTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Verify(bool isSingleThreaded)
    {
        // Arrange

        // XUnit sets a new SynchronizationContext after the first await
        await Task.Yield();
        var previousContext = SynchronizationContext.Current;
        var syncContext = previousContext;

        var threadId = Environment.CurrentManagedThreadId;
        var afterYield = 0;

        async Task request(Http.HttpContext ctx)
        {
            syncContext = SynchronizationContext.Current;

            await Task.Yield();

            afterYield = Environment.CurrentManagedThreadId;
        }

        var middleware = new SingleThreadedRequestMiddleware(request);

        var metadata = new SingleThreadedRequestAttribute { IsDisabled = false };

        var endpoint = new Mock<IEndpointFeature>();
        endpoint.Setup(e => e.Endpoint).Returns(new Endpoint(null, new EndpointMetadataCollection(metadata), null));

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IEndpointFeature>()).Returns(endpoint.Object);

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        // Act
        await middleware.InvokeAsync(context.Object);

        // Assert
        if (isSingleThreaded)
        {
            Assert.NotEqual(previousContext, syncContext);
            Assert.Equal(threadId, afterYield);
        }
        else
        {
            Assert.Equal(previousContext, syncContext);
            Assert.NotEqual(threadId, afterYield);
        }

        Assert.Equal(previousContext, SynchronizationContext.Current);
    }
}
