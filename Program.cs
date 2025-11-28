using MarketPlace.Data;
using MarketPlace.Services;
using Microsoft.EntityFrameworkCore;
using Steeltoe.Discovery.Client;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/actuator/health", () => Results.Ok(new { status = "UP" }));
app.UseMiddleware<GatewayUserMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();