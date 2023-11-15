using DaprDemo;
using FluentHttp.Abstractions;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluentHttp.Test;

[FluentHttp(AppId = "order", Url = "api/v1/order", Name = "OrderFileConsumer", Auth = "jwt")]
public interface IOrderFileConsumer
{
    [HttpFileUpload(AppId = "order", Url = "/api/v1/order/single-file")]
    Task<Order> UploadFileByPath([FilePathVariable] string filepath, Dictionary<string, string> formData);

    [HttpFileUpload(AppId = "order", Url = "/api/v1/order/multi-file")]
    Task<Order> UploadMultipartFileByPath([FilePathVariable] List<string> filepath);
}