using System.IO.Compression;
using System.Text.Json.Serialization;
using Escale.API.Data;
using Escale.API.Data.Seeds;
using Escale.API.Extensions;
using Escale.API.Hubs;
using Escale.API.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// DbContext
builder.Services.AddDbContext<EscaleDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// SignalR
builder.Services.AddSignalR();

// Application services
builder.Services.AddApplicationServices();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Controllers + JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // PascalCase
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// CORS (AllowCredentials required for SignalR WebSocket auth)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "application/json; charset=utf-8"
    });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);

// Memory cache for hot data (org settings, fuel types)
builder.Services.AddMemoryCache();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

// Middleware pipeline
app.UseResponseCompression();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Escale API v1");
    c.RoutePrefix = string.Empty;
    c.DocumentTitle = "Escale Gas Station API";
    c.DefaultModelsExpandDepth(-1);
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<EscaleHub>("/hubs/escale");
app.MapGet("/health", () => Results.Ok("ok")).AllowAnonymous();

// Database migration + seed + EF warm-up
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EscaleDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(db);

    // Warm up EF Core query compilation — forces LINQ-to-SQL translation at startup
    // so first login request doesn't pay the compilation cost (~1-2s)
    var sw = System.Diagnostics.Stopwatch.StartNew();
    _ = await db.Users.Include(u => u.UserStations).ThenInclude(us => us.Station)
        .FirstOrDefaultAsync(u => u.Username == "__warmup__");
    _ = await db.Organizations.FirstOrDefaultAsync(o => o.Id == Guid.Empty);
    _ = await db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "__warmup__");
    Log.Information("EF Core query warm-up completed in {ElapsedMs}ms", sw.ElapsedMilliseconds);
}

Log.Information("Escale API started successfully");

app.Run();
