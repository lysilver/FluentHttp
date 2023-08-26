# 声明式http调用

http调用、支持随机、轮询、加权随机负载均衡

服务健康需要自行实现IServiceDiscovery 动态改变url配置

## 使用方式

- 引入dll

Y.FluentHttp

FluentHttp.SourceGenerator

- DI注入

```csharp
builder.Services.AddHttpClient();
// builder.Services.AddHttpHeader();
// 负载均衡
builder.Services.AddLoadBalancing();
builder.Services.AddFluentHttp(ServiceLifetime.Scoped);

```

实现IContext接口 提供header信息 、json序列化

```csharp
public class CustContext : IContext
    {
        private readonly Func<Dictionary<string, object>> func = () =>
        {
            Dictionary<string, object> keys = new()
                {
                    { "trace_id", Guid.NewGuid().ToString()}
                };
            return keys;
        };

        public DefaultContext()
        {
        }

        // 根据同步的id返回不同的header
        public Task<Dictionary<string, object>> GetHeader(string appId)
        {
            return Task.FromResult(func.Invoke());
        }

        public JsonSerializerOptions? JsonSerializerOptions()
        {
            return null;
        }
    }
```

实现IServiceDiscovery接口 提供url，默认的是从配置文件获取
注入容器，必须在FluentHttp()之前 或者在之后使用services.Replace(ServiceDescriptor.Singleton(typeof(IServiceDiscovery), typeof(CustServiceDiscovery)));

```csharp
public class CustServiceDiscovery : IServiceDiscovery
    {
        private readonly string Node = "YL:FluentHttp";
        private readonly IConfiguration _configuration;

        public DefaultServiceDiscovery(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<Dictionary<string, ClusterConfig>?> GetUrls()
        {
            var services = _configuration.GetSection(Node).Get<Dictionary<string, ClusterConfig>>();
            return Task.FromResult(services);
        }
    }

```

```csharp
// 如果添加了自行替换AddHttpHeader() 使用 
services.Replace(ServiceDescriptor.Singleton(typeof(IContext), typeof(CustContext)));
// 没有添加
services.TryAddSingleton<IContext, CustContext>();
// 放在 builder.Services.AddFluentHttp(ServiceLifetime.Scoped); 之前
// 默认实现的是配置文件 配置文件参考
services.TryAddSingleton<IServiceDiscovery, CustServiceDiscovery>();
```

### 支持请求

#### GET POST DELETE PUT PATCH

- [FluentHttp(AppId = "order", Name = "OrderConsumer", Auth = "jwt")] Name-生成的类名 Auth- 验证的header取值
- [HttpGet(AppId = "order", Url = "api/v1/order/all")]
- [HttpPost(AppId = "order", Url = "api/v1/order/all")]
- [HttpPut(AppId = "order", Url = "api/v1/order/all")]
- [HttpPatch(AppId = "order", Url = "api/v1/order/all")]
- [HttpDelete(AppId = "order", Url = "api/v1/order/all")]
- [HttpFileUpload(AppId = "order", Url = "api/v1/order/all")]
- [HttpResponseMessage(AppId = "order", Url = "/api/v1/order", HttpMethod = HttpMethodEnum.Post)]
- [FilePathVariable]文件路径
- [PathVariable]-url path变量
- AppId-服务的唯一名称
- Url - url
- ReturnString - 返回string 类型

```csharp
[FluentHttp(AppId = "order", Url = "api/v1/order", Name = "OrderConsumer", Auth = "jwt")]
public interface IOrderConsumer
{
    /// <summary>
    /// 获取所有订单
    /// </summary>
    /// <returns></returns>
    [HttpGet(AppId = "order", Url = "all")]
    Task<IEnumerable<Order>> GetOrders();

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

    [HttpGet(AppId = "order", Url = "/api/v1/order/{id}", ReturnString = true)]
    Task<string> GetOrderByIdToString([PathVariable] int id);

    /// <summary>
    /// 创建订单
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    [HttpPost(AppId = "order", Url = "/api/v1/order")]
    Task<Order> CreateOrder(Order order);

    [HttpPut(AppId = "order", Url = "/api/v1/order/{id}")]
    Task<Order> UpdateOrder([PathVariable] int id, Order order);

    [HttpDelete(AppId = "order", Url = "/api/v1/order/{id}")]
    Task<Order> DeleteOrder([PathVariable] int id);

    [HttpFileUpload(AppId = "order", Url = "/api/v1/order/single-file")]
    Task<Order> UploadFileByPath([FilePathVariable] string filepath);

    [HttpFileUpload(AppId = "order", Url = "/api/v1/order/multi-file")]
    Task<Order> UploadMultipartFileByPath([FilePathVariable] List<string> filepath);
}

```

### 配置文件参考

```json
{
  "YL": {
    "FluentHttp": {
      "order": {
        "LoadBalancingPolicy": null,
        "Destinations": [
          {
            "Weight": 6,
            "BaseUrl": "http://localhost:9000"
          },
          {
            "Weight": 2,
            "BaseUrl": "http://localhost:9000"
          }
        ]
      }
    }
  }
}

```

### demo

参考 FluentHttp.Test

### 版本

> 注意 如果更新版本可能需要删除bin obj 重新生成

#### v0.0.1

- [x] HttpGet
- [x] HttpPost
- [x] HttpPut
- [x] HttpDelete
- [x] HttpFileUpload
- [x] 轮询(RoundRobin)
- [x] 随机(Random)
- [x] 加权随机(WeightedRandom)

#### v0.0.2

fix: bug

#### v0.0.3

- [x] HttpPatch
- [x] HttpResponseMessage

#### v0.0.4

sourcelink

