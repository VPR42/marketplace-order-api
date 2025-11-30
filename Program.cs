using MarketPlace.Data;
using MarketPlace.Security;
using MarketPlace.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Steeltoe.Discovery.Client;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});
builder.Services.AddDiscoveryClient(builder.Configuration);
builder.Services.AddLogging();
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddAuthentication("custom")
    .AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>("custom", options => { });
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton<IOrderEventsPublisher, RabbitMqOrderEventsPublisher>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Marketplace Orders API",
        Version = "v1"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseSwagger(c =>
{
    c.RouteTemplate = "api/orders/docs/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api/orders/docs/v1/swagger.json", "Marketplace Orders API v1");
    app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api/orders/docs/v1/swagger.json", "Marketplace Orders API v1");
    c.RoutePrefix = "api/orders/swagger";
});
});

app.MapGet("/api/orders/docs", () =>
        Results.Redirect("/api/orders/docs/v1/swagger.json"))
   .WithMetadata(new AllowAnonymousAttribute());

app.MapGet("/actuator/health", () => Results.Ok(new { status = "UP" }));
app.UseMiddleware<GatewayUserMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
