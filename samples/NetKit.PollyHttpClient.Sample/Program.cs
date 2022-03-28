using NetKit.HttpClientExtension;
using NetKit.PollyHttpClient.Sample;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

HtttpRetryExtension.Config();

// Using HttpClient
builder.Services.AddPollyClient<PublicClient>("https://api.publicapis.org/");

// Using Refit Client
builder.Services.AddPollyRefitClient<IPublicApis>("https://api.publicapis.org");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
