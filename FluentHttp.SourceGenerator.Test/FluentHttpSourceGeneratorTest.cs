using FluentHttp.Abstractions;
using Xunit;

namespace FluentHttp.SourceGenerator.Test
{
    public class FluentHttpSourceGeneratorTest
    {
        private ITestOutputHelper testOutput { get; }

        public FluentHttpSourceGeneratorTest(ITestOutputHelper testOutputHelper)
        {
            testOutput = testOutputHelper;
        }

        [Fact]
        public void TestFluentHttpGenerator()
        {
            string source = """
                using FluentHttp.Abstractions;
                using System.Collections.Generic;
                using System.Net.Http;
                using System.Threading.Tasks;

                namespace FluentHttp.Test;

                [FluentHttp(AppId = "order", Url = "api/v1/order", Name = "OrderConsumer", ClientName = "OrderClient")]
                public interface IOrderConsumer
                {
                    /// <summary>
                    /// 获取所有订单
                    /// </summary>
                    /// <returns></returns>
                    [HttpGet(AppId = "order", Url = "all")]
                    Task<IEnumerable<Order>> GetOrders();

                    [HttpGet(AppId = "order", Url = "all")]
                    ValueTask<IEnumerable<Order>> GetOrdersAsync();

                    /// <summary>
                    /// 获取所有订单
                    /// 接口上的AppId优先级低于方法上的
                    /// 前缀/开头的Url不会拼接接口上的Url
                    /// </summary>
                    /// <returns></returns>
                    [HttpGet(Url = "/api/v1/order/all")]
                    Task<IEnumerable<Order>> GetAllOrders();

                    /// <summary>
                    /// 根据id获取订单
                    /// </summary>
                    /// <param name="id"></param>
                    /// <returns></returns>
                    [HttpGet(AppId = "order", Url = "/api/v1/order/{id}")]
                    Task<Order> GetOrderById([PathVariable] int id);

                    [HttpGet(AppId = "order", Url = "/api/v1/order/{id}", HttpReturnType = HttpReturnType.String)]
                    Task<string> GetOrderByIdToString([PathVariable] int id);

                    /// <summary>
                    /// 创建订单
                    /// </summary>
                    /// <param name="order"></param>
                    /// <returns></returns>
                    [HttpPost(AppId = "order", Url = "/api/v1/order")]
                    Task<Order> CreateOrder(Order order);

                    [HttpResponseMessage(AppId = "order", Url = "/api/v1/order", HttpMethod = HttpMethodType.Post)]
                    Task<HttpResponseMessage> CreateOrder_2(Order order);

                    [HttpPut(AppId = "order", Url = "/api/v1/order/{id}")]
                    Task<Order> UpdateOrder([PathVariable] int id, Order order);

                    [HttpPatch(AppId = "order", Url = "/api/v1/order/{id}")]
                    Task<Order> UpdatePatchOrder([PathVariable] int id, Order order);

                    [HttpPatch(AppId = "order", Url = "/api/v1/order/{id}", UseJsonPatch = false)]
                    Task<Order> UpdatePatchOrderJson([PathVariable] int id, Order order);

                    [HttpDelete(AppId = "order", Url = "/api/v1/order/{id}")]
                    Task<Order> DeleteOrder([PathVariable] int id);

                    [HttpFileUpload(AppId = "order", Url = "/api/v1/order/single-file")]
                    Task<Order> UploadFileByPath([FilePathVariable] string filepath);

                    [HttpFileUpload(AppId = "order", Url = "/api/v1/order/multi-file")]
                    Task<Order> UploadMultipartFileByPath([FilePathVariable] List<string> filepath);
                }
                """;
            var harness = new SourceGeneratorTestHarness(source)
                .AddGenerator<HttpIncrementalGenerator>()
                .AddReferenceType(typeof(FluentHttpAttribute));
            var result = harness.Run();
            var code = string.Join(Environment.NewLine, result.GeneratedTrees.Select(static t => t.GetText().ToString()));

            Assert.Contains("public partial class OrderConsumer : IOrderConsumer", code);
            Assert.Contains("using var client = _clientFactory.CreateClient(\"OrderClient\");", code);
            Assert.Contains("client.HttpGet<IEnumerable<Order>>(_clientAdapter, \"order\", \"api/v1/order/all\")", code);
            Assert.Contains("public async ValueTask<IEnumerable<Order>> GetOrdersAsync()", code);
            Assert.Contains("using var res = await client.HttpResponseMessageAsync<object>(_clientAdapter, \"order\", $\"api/v1/order/{id}\", HttpMethod.Get, null);", code);
            Assert.Contains("return await res.Content.ReadAsStringAsync();", code);
            Assert.Contains("client.HttpGet<Order>(_clientAdapter, \"order\", $\"api/v1/order/{id}\")", code);
            Assert.Contains("client.HttpPost<Order, Order>(_clientAdapter, \"order\", \"api/v1/order\", order)", code);
            Assert.Contains("client.HttpPatch<Order, Order>(_clientAdapter, \"order\", $\"api/v1/order/{id}\", order, contentType: HttpContentType.Json)", code);
            Assert.Contains("client.HttpResponseMessageAsync<Order>(_clientAdapter, \"order\", \"api/v1/order\", HttpMethod.Post, order)", code);
            Assert.Contains("client.HttpDelete<Order>(_clientAdapter, \"order\", $\"api/v1/order/{id}\")", code);
            Assert.Contains("client.UploadFile<Order>(_clientAdapter, \"order\", \"api/v1/order/single-file\", filepath)", code);
            Assert.Contains("services.TryAdd(ServiceDescriptor.Describe(typeof(FluentHttp.Test.IOrderConsumer), typeof(FluentHttp.Test.OrderConsumer), lifetime))", code);
        }
    }
}
