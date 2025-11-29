using MarketPlace.Data;
using MarketPlace.Security;
using MarketPlace.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Steeltoe.Discovery.Client;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

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
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
    });
    app.UseSwaggerUI();
}

app.MapGet("/actuator/health", () => Results.Ok(new { status = "UP" }));
app.UseMiddleware<GatewayUserMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();