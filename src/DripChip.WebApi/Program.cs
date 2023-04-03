using DripChip.WebApi.Setup;
using DripChip.WebApi.Setup.Auth;
using DripChip.WebApi.Setup.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Personal.json", true);

builder.SetupControllers();
builder.SetupSwagger();
builder.SetupLogging();
builder.SetupDb();
builder.SetupAuth();
builder.SetupExceptionHandling();
builder.SetupDomain();

var app = builder.Build();

await app.ApplyMigrations();
await app.SeedDb();
app.UseHealthCheckSetup();
app.UseLoggingSetup();
app.UseExceptionHandlingSetup();
app.UseSwaggerSetup();

//app.UseHttpsRedirection();
app.UseAuthSetup();
app.UseControllersSetup();

app.Run();
