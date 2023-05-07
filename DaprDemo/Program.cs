var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});
builder.Services.AddHttpClient();
builder.Services.AddHttpHeader();
builder.Services.AddFluentHttp(ServiceLifetime.Scoped);
builder.Services.AddFluentHttpPool();
// ¸ºÔØ¾ùºâ
builder.Services.AddLoadBalancing();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthorization();

app.MapControllers();

app.Run();