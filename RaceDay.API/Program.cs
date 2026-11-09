using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// EF Core + SQL Server, Code-First against the same schema as the Part 1 SQL script.
builder.Services.AddDbContext<RaceDayContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDayConnection")));

// Password hashing lives behind an interface so it's injected rather than a field
// new'd up inside AuthController - also makes it swappable/testable on its own.
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();

// Azure Blob Storage for event banners and profile pictures - see BlobStorageService
// for why it doesn't connect to Azure until UploadFileAsync is actually called.
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// Session-based auth: no JWT here, just a server-side session that remembers
// who logged in and what role they picked at registration.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "Event management API for South African running, walking and cycling events."
    });

    // Pulls in the /// <summary> comments from the controllers so Swagger shows
    // real descriptions instead of just raw routes.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Allows a separate front-end (Part 3) to call this API with cookies/session attached.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Apply EF Core migrations before any database queries. This creates the tables
// in a new RaceDay database and applies pending schema changes on later starts.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
    await context.Database.MigrateAsync();

    // Demo accounts are optional, but the schema must exist before seeding.
    if (app.Configuration.GetValue("SeedDemoData", true))
    {
        var passwordHashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();
        await SeedData.SeedAsync(context, passwordHashService);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

// UseSession has to sit before anything that reads HttpContext.Session.
app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed so RaceDay.Tests can spin the API up in-memory if needed later.
public partial class Program { }
