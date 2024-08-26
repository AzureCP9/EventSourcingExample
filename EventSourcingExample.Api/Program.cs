using EventSourcingExample.Api.Db;
using EventSourcingExample.Api.Resilience;
using EventSourcingExample.Api.Services;
using EventStore.Client;
using Microsoft.EntityFrameworkCore;
using Polly.Retry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEventStoreClient(options =>
{
    options.ConnectivitySettings.Address = new Uri("http://eventstoredb:2113");
    options.DefaultCredentials = new UserCredentials("admin", "changeit");
    options.ConnectivitySettings.Insecure = true;
});

builder.Services.AddScoped<IRetryPolicy, EventRetryPolicy>();

builder.Services.AddSqlServer<ToyDbContext>("Server=readdb;Database=ToysDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;");

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddSingleton<ProjectionPolicy>();

builder.Services.AddScoped<ToyService>();
builder.Services.AddHostedService<ToyProjectionService>();
builder.Services.AddSingleton<ToyProjectionService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var toyDbContext = scope.ServiceProvider.GetRequiredService<ToyDbContext>();
    toyDbContext.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();