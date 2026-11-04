using System.Text.Json.Serialization;
using RequestsManagement.DAL.Data;
using RequestsManagement.DAL.Repositories;
using RequestsManagement.API.Middleware;
using RequestsManagement.BL.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache(options => options.SizeLimit = 1000);

builder.Services.AddScoped<IRequestRepository, RequestRepository>();

builder.Services.AddScoped<RequestService>();
builder.Services.AddScoped<IRequestService, CachedRequestService>();
builder.Services.AddScoped<DataSeeder>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync(reset: args.Contains("--reset"));
    return;
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program { }
