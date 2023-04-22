using DaprDemo;
using FluentHttp.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentHttp.Test;

[FluentHttp(AppId = "order", Name = "OrderConsumer", Auth = "jwt")]
public interface IOrderConsumer
{
    /// <summary>
    /// 获取所有订单
    /// </summary>
    /// <returns></returns>
    [HttpGet(AppId = "order", Url = "api/v1/order/all")]
    Task<IEnumerable<Order>> GetOrders();

    /// <summary>
    /// 获取所有订单
    /// 接口上的AppId优先级低于方法上的
    /// </summary>
    /// <returns></returns>
    [HttpGet(Url = "api/v1/order/all")]
    Task<IEnumerable<Order>> GetAllOrders();

    /// <summary>
    /// 根据id获取订单
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet(AppId = "order", Url = "api/v1/order/{id}")]
    Task<Order> GetOrderById([PathVariable] int id);

    [HttpGet(AppId = "order", Url = "api/v1/order/{id}", ReturnString = true)]
    Task<string> GetOrderByIdToString([PathVariable] int id);

    /// <summary>
    /// 创建订单
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    [HttpPost(AppId = "order", Url = "api/v1/order")]
    Task<Order> CreateOrder(Order order);

    [HttpPut(AppId = "order", Url = "api/v1/order/{id}")]
    Task<Order> UpdateOrder([PathVariable] int id, Order order);

    [HttpDelete(AppId = "order", Url = "api/v1/order/{id}")]
    Task<Order> DeleteOrder([PathVariable] int id);

    [HttpFileUpload(AppId = "order", Url = "api/v1/order/single-file")]
    Task<Order> UploadFileByPath([FilePathVariable] string filepath);

    [HttpFileUpload(AppId = "order", Url = "api/v1/order/multi-file")]
    Task<Order> UploadMultipartFileByPath([FilePathVariable] List<string> filepath);
}