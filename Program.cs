using Asp.Versioning;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using ServiceDiscovery.Services;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel();

Log.Logger = new LoggerConfiguration()
   .ReadFrom.Configuration(builder.Configuration)
   .Enrich.FromLogContext()
   .CreateLogger();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
   options.SwaggerDoc("v1", new OpenApiInfo {
      Title = "eBank Service Discovery Service",
      Description = "Service Discovery microservice",
      Version = "1",
   });
   options.EnableAnnotations();
});
builder.Services.AddHttpClient();
builder.Services.UseHttpClientMetrics();
builder.Services.AddApiVersioning(options => {
   options.ReportApiVersions = true;
   options.AssumeDefaultVersionWhenUnspecified = true;
   options.DefaultApiVersion = new ApiVersion(1);
   options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options => {
   options.GroupNameFormat = "'v'V";
   options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddSingleton<RegistryService>();
builder.Services.AddSingleton<LoadBalancingService>();
builder.Services.AddSerilog();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
SetupRedis();

WebApplication app = builder.Build();

app.UseSerilogRequestLogging();
app.UseSwagger(options => {
   options.RouteTemplate = "Api/Docs/{documentName}/swagger.json";
});
app.UseSwaggerUI(options => {
   options.SwaggerEndpoint("/Api/Docs/v1/swagger.json", "Service Discovery v1");
   options.DocumentTitle = "Service Discovery Docs";
   options.RoutePrefix = "Api/Docs";
});
app.UseMetricServer();
app.UseHttpMetrics();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHealthChecks("/healthz");
app.MapControllers();

Run();

return;

void Run() {
   string scheme = Environment.GetEnvironmentVariable("HTTP_SCHEME")!;
   string host = Environment.GetEnvironmentVariable("HTTP_HOST")!;
   string port = Environment.GetEnvironmentVariable("HTTP_PORT")!;
   string url = $"{scheme}://{host}:{port}";

   app.Run(url);
}

void SetupRedis() {
   string host = Environment.GetEnvironmentVariable("REDIS_HOST")!;
   string port = Environment.GetEnvironmentVariable("REDIS_PORT")!;
   int db = int.Parse(Environment.GetEnvironmentVariable("REDIS_DB")!);
   string user = Environment.GetEnvironmentVariable("REDIS_USER")!;
   string password = Environment.GetEnvironmentVariable("REDIS_PASSWORD")!;

   var options = new ConfigurationOptions {
      Protocol = RedisProtocol.Resp3,
      EndPoints = { $"{host}:{port}" },
      Password = password,
      User = user,
      DefaultDatabase = db,
      Ssl = false,
      AllowAdmin = false,
      ConnectRetry = 3,
      ConnectTimeout = 5000,
   };

   builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(options));
}
