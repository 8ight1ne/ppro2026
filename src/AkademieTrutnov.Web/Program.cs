using AkademieTrutnov.Core.Interfaces;
using AkademieTrutnov.Core.Services;
using AkademieTrutnov.Infrastructure.Data;
using AkademieTrutnov.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Databáze (PostgreSQL pro Docker/produkci, SQLite pro lokální běh bez Dockeru)
var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSql";
var postgresConn = builder.Configuration.GetConnectionString("PostgresConnection");
var sqliteConn = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=akademietrutnov.db";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (dbProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(postgresConn);
    }
    else
    {
        options.UseSqlite(sqliteConn);
    }
});

// 2. Registrace repozitářů a UnitOfWork (Infrastrukturní vrstva)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITerminRepository, TerminRepository>();
builder.Services.AddScoped<IPrihlaskaRepository, PrihlaskaRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IKurzRepository, KurzRepository>();
builder.Services.AddScoped<IOsvedceniRepository, OsvedceniRepository>();
builder.Services.AddScoped<ILektorRepository, LektorRepository>();
builder.Services.AddScoped<IDochazkaRepository, DochazkaRepository>();

// 3. Registrace doménových služeb (Aplikační/Doménová vrstva)
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IReportingService, ReportingService>();

// 4. Prezentační vrstva: Razor Pages & REST API Controllers + Swagger
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Akademie Trutnov API",
        Version = "v1",
        Description = "REST API informačního systému pro správu kurzů, termínů, pořadníku a osvědčení (Zadání C - PPRO 2026/2027)"
    });
});

var app = builder.Build();

// 5. Automatická inicializace databáze a seed syntetických dat při spuštění
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        logger.LogInformation("Inicializuji databázi ({Provider})...", dbProvider);
        if (dbProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }
        logger.LogInformation("Spouštím seed syntetických dat...");
        await DatabaseSeeder.SeedAsync(db);
        logger.LogInformation("Databáze je připravena.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Chyba při inicializaci nebo seedování databáze.");
    }
}

// 6. HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Akademie Trutnov API v1");
    c.RoutePrefix = "swagger";
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
