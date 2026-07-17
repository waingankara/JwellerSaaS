using JwellerSaaS.Api.Middleware;
using JwellerSaaS.Api.Security;
using JwellerSaaS.Api.Tenancy;
using JwellerSaaS.Application.DependencyInjection;
using JwellerSaaS.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Orion.Framework.Security;
using Orion.Framework.Tenancy;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console());

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();
builder.Services.AddScoped<ITenantContextAccessor, HttpTenantContextAccessor>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync().ConfigureAwait(false);
