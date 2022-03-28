1. Install NetKit.OpenTelemetry package:
```
PM> Install-Package NetKit.Swashbuckle
```
2. Update appsettings with this configuration:
```
{
	"Swagger": {
        "Title": "Demo Swashbuckle",
        "Description": "Demo Swashbuckle",
        "Version": "v1"
    }
}
```
3. In Programs.cs, add these line to configure Swachbuckle
```
builder.Services.AddNetKitSwashbuckle(builder.Configuration);
```
    - If you want to add JWT security header, use this code:
    ```
    builder.Services.AddNetKitSwashbuckle(builder.Configuration, options=> {
        options.AddJwtSecurityDefinition();
    });
    ```
```
app.UseNetKitSwashbuckle();
```