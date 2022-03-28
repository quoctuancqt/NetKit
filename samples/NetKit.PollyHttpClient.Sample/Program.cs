using NetKit.HttpClientExtension;
using NetKit.PollyHttpClient.Sample;
using NetKit.Swashbuckle;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddNetKitSwashbuckle(builder.Configuration, options =>
{
    // Uncomment to use JWT security header
    // options.AddJwtSecurityDefinition();
});

// Uncomment to configure AuthService to check access swagger endpoint
// builder.Services.AddSwashbuckleAuthService<SwaggerAuthService>();

HtttpRetryExtension.Config();

// Using HttpClient
builder.Services.AddPollyClient<PublicClient>("https://api.publicapis.org/");

// Using Refit Client
builder.Services.AddPollyRefitClient<IPublicApis>("https://api.publicapis.org");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Uncomment to use authorize swagger endpoint
    // app.UseSwaggerAuthorized();

    app.UseNetKitSwashbuckle();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
