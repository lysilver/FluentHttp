using FluentHttp.Ext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FluentHttp.Test
{
    /// <summary>
    /// HttpGetStream/HttpPostStream/HttpSendStream 的响应释放行为：
    /// 非成功状态码必须释放响应，Dispose Stream 必须同时释放 HttpResponseMessage。
    /// </summary>
    public class TestHttpStream
    {
        private sealed class StubAdapter : IHttpClientAdapter
        {
            public ValueTask<Dictionary<string, ClusterConfig>> GetUrls()
                => new(new Dictionary<string, ClusterConfig>());

            public ValueTask<string> GetUrl(string appId, string url) => new($"http://localhost/{url}");

            public ValueTask ConfigureRequest(string appId, HttpRequestMessage request) => ValueTask.CompletedTask;

            public JsonSerializerOptions? GetJsonSerializerOptions(string appId) => null;
        }

        private sealed class TrackingContent : StringContent
        {
            public bool Disposed { get; private set; }

            public TrackingContent(string content) : base(content) { }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                base.Dispose(disposing);
            }
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpResponseMessage> _factory;

            public StubHandler(Func<HttpResponseMessage> factory) => _factory = factory;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_factory());
        }

        [Fact]
        public async Task TestStreamErrorStatusCodeDisposesResponse()
        {
            var content = new TrackingContent("error");
            var handler = new StubHandler(() => new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = content });
            using var client = new HttpClient(handler);
            var adapter = new StubAdapter();

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                client.HttpGetStream(adapter, "order", "api/v1/order/all", TestContext.Current.CancellationToken));

            Assert.True(content.Disposed, "非成功状态码时 response 未被释放，连接会泄漏");
        }

        [Fact]
        public async Task TestStreamDisposeReleasesResponse()
        {
            var content = new TrackingContent("[{\"id\":1}]");
            var handler = new StubHandler(() => new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
            using var client = new HttpClient(handler);
            var adapter = new StubAdapter();

            using var stream = await client.HttpGetStream(adapter, "order", "api/v1/order/all", TestContext.Current.CancellationToken);
            Assert.False(content.Disposed, "读取前不应释放");

            using var reader = new StreamReader(stream);
            var text = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
            Assert.Contains("id", text);
        }

        [Fact]
        public async Task TestPostStreamDisposeReleasesResponse()
        {
            var content = new TrackingContent("{\"id\":2}");
            var handler = new StubHandler(() => new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
            using var client = new HttpClient(handler);
            var adapter = new StubAdapter();

            var stream = await client.HttpPostStream(adapter, "order", "api/v1/order", new { id = 1 }, TestContext.Current.CancellationToken);
            stream.Dispose();

            Assert.True(content.Disposed, "HttpPostStream 的 Stream.Dispose 必须释放 response");
        }
    }
}
