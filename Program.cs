using MarketPlace.Data;
using MarketPlace.Security;
using MarketPlace.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Steeltoe.Discovery.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDiscoveryClient(builder.Configuration);
builder.Services.AddLogging();
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<HeaderAuthenticationMiddleware>();

builder.Services.AddAuthentication("custom")
    .AddScheme<AuthenticationSchemeOptions, EmptyAuthHandler>("custom", options => { });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder("custom")
        .RequireAuthenticatedUser()
        .Build();
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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<HeaderAuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();