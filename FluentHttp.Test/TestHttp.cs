using FluentHttp.Ext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using YL.Extensions.DependencyInjection;

namespace FluentHttp.Test
{
    public class TestHttp
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IOrderConsumer orderConsumer;
        private readonly IOrderFileConsumer orderFileConsumer;
        private readonly IContext context;
        private readonly string Node = "YL:FluentHttp";

        public TestHttp()
        {
            var services = new ServiceCollection();
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json");
            IConfiguration configuration = builder.Build();
            services.AddScoped(_ => configuration);
            services.AddHttpClient();
            services.AddDefaultFluentHttp();
            services.AddFluentHttp();
            var serviceProvider = services.BuildServiceProvider();
            httpClientFactory = (IHttpClientFactory)serviceProvider.GetRequiredService(typeof(IHttpClientFactory));
            orderConsumer = (IOrderConsumer)serviceProvider.GetRequiredService(typeof(IOrderConsumer));
            context = (IContext)serviceProvider.GetRequiredService(typeof(IContext));
            context.GetHeader("order");
            orderFileConsumer = (IOrderFileConsumer)serviceProvider.GetRequiredService(typeof(IOrderFileConsumer));
            var clusters = configuration.GetSection(Node).Get<Dictionary<string, ClusterConfig>>();
        }

        [Fact]
        public async Task TestGet()
        {
            var id = 2;
            var res = await orderConsumer.GetOrderById(id);
            Assert.Equal(res.Id, id);
        }

        [Fact]
        public async Task TestGetOrderByIdToString()
        {
            var id = 6;
            var res = await orderConsumer.GetOrderByIdToString(id);
            Assert.Contains("6", res);
        }

        [Fact]
        public async Task TestGetAll()
        {
            var res = await orderConsumer.GetOrders();
            Assert.Equal(5, res.Count());
        }

        [Fact]
        public async Task TestGetAll_2()
        {
            var res = await orderConsumer.GetAllOrders();
            Assert.Equal(5, res.Count());
        }

        [Fact]
        public async Task TestPost()
        {
            var order = new Order
            {
                Id = 2,
                Code = "测试",
                Date = DateTime.Now
            };
            var res = await orderConsumer.CreateOrder(order);
            Assert.Equal(order.Id, res.Id);
        }

        [Fact]
        public async Task TestHttpReponseMessage()
        {
            var order = new Order
            {
                Id = 2,
                Code = "测试",
                Date = DateTime.Now
            };
            var responseMessage = await orderConsumer.CreateOrder_2(order);
            var res = await responseMessage.Content.ReadFromJsonAsync<Order>();
            Assert.True(responseMessage.IsSuccessStatusCode);
            Assert.NotNull(res);
            Assert.Equal(order.Id, res.Id);
        }

        [Fact]
        public async Task TestPut()
        {
            var id = 6;
            var order = new Order
            {
                Id = 2,
                Code = "测试",
                Date = DateTime.Now
            };
            var res = await orderConsumer.UpdateOrder(id, order);
            Assert.Equal(id, res.Id);
        }

        [Fact]
        public async Task TestPatch()
        {
            var id = 6;
            var order = new Order
            {
                Id = 2,
                Code = "测试",
                Date = DateTime.Now
            };
            var res = await orderConsumer.UpdatePatchOrder(id, order);
            Assert.Equal(id, res.Id);
        }

        [Fact]
        public async Task TestDelete()
        {
            var id = 6;
            var res = await orderConsumer.DeleteOrder(id);
            Assert.Equal(id, res.Id);
            Assert.Equal("删除成功", res.Code);
        }

        [Fact]
        public void TestGetUrl()
        {
            var dict = new Dictionary<string, object>()
            {
                {"code", 1},
                {"name", 1}
            };
            var properties = from p in dict
                             where p.Value != null
                             select p.Key + "=" + p.Value.ToString() + "";
            var url = string.Join("&", properties.ToArray());
            Assert.Equal("code=1&name=1", url);
        }

        [Fact]
        public void TestFunc()
        {
            string key = "trace_id";
            Dictionary<string, object> func()
            {
                Dictionary<string, object> keys = new()
                {
                    { key, Guid.NewGuid().ToString()}
                };
                return keys;
            }
            func().TryGetValue(key, out object? traceId1);
            func().TryGetValue(key, out object? traceId2);
            Assert.NotEqual(traceId1, traceId2);
        }

        [Fact]
        public async Task TestUploadFile()
        {
            string file = await CreateFile();
            var res = await orderConsumer.UploadFileByPath(file);
            DeleteFile(file);
            Assert.Equal(0, res.Id);
        }

        [Fact]
        public async Task TestMultipartUploadFile()
        {
            List<string> files = await CreateFiles();
            var res = await orderConsumer.UploadMultipartFileByPath(files);
            Assert.Equal(0, res.Id);
        }

        [Fact]
        public async Task TestMultiFile()
        {
            using var httpClient = httpClientFactory.CreateClient();
            using var multipartFormContent = new MultipartFormDataContent();
            string file = await CreateFile();
            var url = "http://localhost:9000/api/v1/order/multi-file";
            var byteContent = new StreamContent(File.Open(file, FileMode.Open, FileAccess.Read));
            multipartFormContent.Add(byteContent, name: "files", fileName: file);
            var response = await httpClient.PostAsync(url, multipartFormContent);
            response.EnsureSuccessStatusCode();
            var res = await response.Content.ReadFromJsonAsync<Order>();
            Assert.Equal(0, res?.Id);
        }

        [Fact]
        public async Task TestMulti()
        {
            List<string> files = await CreateFiles();
            var url = "http://localhost:9000/api/v1/order/multi-file";
            using var httpClient = httpClientFactory.CreateClient();
            using MultipartFormDataContent content = new MultipartFormDataContent();
            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                var scontent = new StreamContent(fileStream, (int)fileStream.Length);
                scontent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                content.Add(scontent, "files", fileName);
            }
            var response = await httpClient.PostAsync(url, content);
            var res = await response.Content.ReadFromJsonAsync<Order>();
            Assert.Equal(0, res?.Id);
        }

        private void DeleteFile(string filepath)
        {
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }

        private async Task<string> CreateFile()
        {
            string file = Guid.NewGuid().ToString() + ".txt";
            var fs = File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.Position = fs.Length;
            var bytes = Encoding.UTF8.GetBytes(file);
            fs.Write(bytes, 0, bytes.Length);
            await fs.FlushAsync();
            fs.Close();
            return file;
        }

        private async Task<List<string>> CreateFiles(int count = 10)
        {
            List<string> files = new();
            for (int i = 0; i < count; i++)
            {
                var file = await CreateFile();
                files.Add(file);
            }
            return files;
        }

        [Fact]
        public async Task TestUploadFileFormData()
        {
            string file = await CreateFile();
            var data = new Dictionary<string, string>()
            {
                {"remark","remark" },
                {"bucket","bucket" },
                {"source","source" }
            };
            var res = await orderFileConsumer.UploadFileByPath(file, data);
            DeleteFile(file);
            Assert.Equal(0, res.Id);
        }
    }
}