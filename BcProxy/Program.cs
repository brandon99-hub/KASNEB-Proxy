using BcProxy.Middleware;
using BcProxy.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "KASNEB BC Proxy API",
        Version = "v1",
        Description = "Proxy layer between Business Central and the KASNEB CRM — exposes student bio-data, exam accounts, and ledger entries."
    });
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Key authentication using X-API-Key header",
        Name = "X-API-Key",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─── HttpClient for ODataFetcher ─────────────────────────────────────────────
// Uses Windows Authentication (NTLM/Negotiate) via UseDefaultCredentials — the
// proxy runs on the same domain as the BC server so the machine account is used.
builder.Services.AddHttpClient<ODataFetcher>(client =>
{
    var baseUrl = builder.Configuration["BusinessCentral:BaseUrl"]
        ?? throw new InvalidOperationException("BusinessCentral:BaseUrl is not configured in appsettings.json");

    if (!baseUrl.EndsWith("/")) baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);

    // 90-second timeout to accommodate large paginated pulls from BC
    client.Timeout = TimeSpan.FromSeconds(90);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseDefaultCredentials = true   // Windows Auth — NTLM/Negotiate
});

// ─── Application services ─────────────────────────────────────────────────────
builder.Services.AddScoped<StudentProfileService>();
builder.Services.AddScoped<InstitutionService>();
builder.Services.AddScoped<VendorService>();

// ─── Logging ─────────────────────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "KASNEB BC Proxy v1");
    c.RoutePrefix = "swagger";
});

app.UseMiddleware<ApiKeyMiddleware>();

// app.UseHttpsRedirection(); // Disabled — plain HTTP on internal network
app.UseAuthorization();
app.MapControllers();

app.Run();
