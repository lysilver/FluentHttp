using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System.Net;

namespace DaprDemo.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<OrderController> _logger;
        private readonly IHttpClientFactory clientFactory;
        private readonly IWebHostEnvironment environment;

        public OrderController(ILogger<OrderController> logger, IHttpClientFactory clientFactory, IWebHostEnvironment environment)
        {
            _logger = logger;
            this.clientFactory = clientFactory;
            this.environment = environment;
        }

        /// <summary>
        /// 获取所有订单
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<ActionResult> GetAll()
        {
            var res = await Task.FromResult(Enumerable.Range(1, 5).Select(index => new Order
            {
                Date = DateTime.Now.AddDays(index),
                Id = Random.Shared.Next(-20, 55),
                Code = Summaries[Random.Shared.Next(Summaries.Length)]
            })
              .ToArray());
            return Ok(res);
        }

        /// <summary>
        /// 根据id获取订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetOrderById(int id)
        {
            var traceId = HttpContext.Request.Headers["trace_id"].ToString();
            _logger.LogInformation("traceId-{}", traceId);
            HttpContext.Request.Headers.ToList().ForEach(c =>
            {
                _logger.LogInformation($"{c.Key}-{c.Value}");
            });
            var res = await Task.FromResult(new Order
            {
                Id = id,
                Code = traceId,
                Date = DateTime.Now,
            });
            return Ok(res);
        }

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost()]
        public async Task<ActionResult> CreateOrder(Order order)
        {
            order = await Task.FromResult(order);
            return Ok(order);
        }

        /// <summary>
        /// 修改订单信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, Order order)
        {
            order.Id = id;
            order = await Task.FromResult(order);
            return Ok(order);
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            _logger.LogInformation("delete-id-{}", id);
            var traceId = HttpContext.Request.Headers["trace_id"].ToString();
            _logger.LogInformation("traceId-{}", traceId);
            var res = await Task.FromResult(Tuple.Create(id, "删除成功"));
            return Ok(new Order
            {
                Id = id,
                Code = "删除成功",
                Date = DateTime.Now,
            });
        }

        [HttpPost("single-file"), DisableRequestSizeLimit]
        public async Task<IActionResult> UploadSingleFile(IFormFile file)
        {
            var path = environment.ContentRootPath;
            path = Path.Combine(path, "upload");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (file == null || file.Length <= 0)
            {
                return Ok(new Order { Id = 1, Code = "文件为空" });
            }
            string fileExt = Path.GetExtension(file.FileName).ToLower();
            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            using var stream = new FileStream(Path.Combine(path, $"{Guid.NewGuid()}-{fileName}{fileExt}"), FileMode.Create);
            await file.CopyToAsync(stream);
            return Ok(new Order { Id = 0, Code = file.FileName });
        }

        [HttpPost("multi-file"), DisableRequestSizeLimit]
        public async Task<IActionResult> UploadMultiFile(List<IFormFile> files)
        {
            var path = environment.ContentRootPath;
            path = Path.Combine(path, "upload");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (files == null || files.Count <= 0)
            {
                return Ok(new Order { Id = 1, Code = "文件为空" });
            }
            foreach (var file in files)
            {
                string fileExt = Path.GetExtension(file.FileName).ToLower();
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                using var stream = new FileStream(Path.Combine(path, $"{Guid.NewGuid()}-{fileName}{fileExt}"), FileMode.Create);
                await file.CopyToAsync(stream);
            }
            return Ok(new Order { Id = 0, Code = files.Count + "" });
        }

        [HttpGet("download-file")]
        public IActionResult DownloadFile(string fileName)
        {
            var path = environment.ContentRootPath;
            path = Path.Combine(path, "upload", fileName);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }
            return File(path, GetMimeType(fileName), true);
        }

        [HttpGet("file-info")]
        public IActionResult GetFileInfo(string fileName)
        {
            var path = environment.ContentRootPath;
            path = Path.Combine(path, "upload", fileName);
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("找不到文件");
            }
            long fileSize = fileInfo.Length;
            return Ok(fileSize);
        }

        [NonAction]
        private string GetMimeType(string filePath)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out string? contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}