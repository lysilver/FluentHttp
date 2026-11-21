using FluentHttp.Ext;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});
builder.Services
    .AddHttpClient()
    .ConfigureHttpClientDefaults(http =>
    {
        http.ConfigureHttpClient(client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        });
        http.ConfigurePrimaryHttpMessageHandler(() =>
            new SocketsHttpHandler
            {
                AutomaticDecompression =
                    DecompressionMethods.Zstandard |
                    DecompressionMethods.Brotli |
                    DecompressionMethods.GZip |
                    DecompressionMethods.Deflate,
                AllowAutoRedirect = false,
                UseCookies = false,
                // 连接池中的连接最长生命周期
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),

                // 空闲连接回收时间
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                ConnectTimeout = TimeSpan.FromSeconds(5),
            });
    });

builder.Services.AddResponseCompression();
builder.Services.AddDefaultFluentHttp();
//builder.Services.AddFluentHttp(ServiceLifetime.Scoped);
builder.Services.AddFluentHttpPool();
// 负载均衡
var app = builder.Build();
app.UseResponseCompression();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthorization();

app.MapControllers();

app.Run();