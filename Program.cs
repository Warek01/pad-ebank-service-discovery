using Asp.Versioning;
using Serilog;
using ServiceDiscovery.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel();
Log.Logger = new LoggerConfiguration()
  .ReadFrom.Configuration(builder.Configuration)
  .Enrich.FromLogContext()
  .CreateLogger();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
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
builder.Services.AddSingleton<HealthCheckService>();
builder.Services.AddSerilog();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

string httpScheme = Environment.GetEnvironmentVariable("HTTP_SCHEME")!;
string httpHost = Environment.GetEnvironmentVariable("HTTP_HOST")!;
string httpPort = Environment.GetEnvironmentVariable("HTTP_PORT")!;
string httpUrl = $"{httpScheme}://{httpHost}:{httpPort}";

app.Run(httpUrl);
