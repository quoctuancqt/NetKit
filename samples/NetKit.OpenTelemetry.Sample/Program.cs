using NetKit.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

var openTelemetryInfo = OpenTelemetryExtensions.GetResourceBuilder(builder.Configuration, "AspNetCoreExampleService");

builder.Services.AddNetKitOpenTelemetryTracing(builder.Configuration, openTelemetryInfo);

builder.Services.AddNetKitOpenTelemetryLogging(builder.Configuration, openTelemetryInfo);

builder.Services.AddNetKitOpenTelemetryMetrics(builder.Configuration, openTelemetryInfo);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseNetKitOpenTelemetryMetricsExporter(builder.Configuration);

app.Run();
