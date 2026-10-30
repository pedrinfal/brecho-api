using System.Text;
using Brecho.Api.Dados;
using Brecho.Api.Servicos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Padrao");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure a variável de ambiente ConnectionStrings__Padrao no formato Npgsql (Host=...;Database=...;Username=...;Password=...;SSL Mode=VerifyFull;Channel Binding=Require).");
}

if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
    connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "A connection string está no formato URI (postgresql://). Use o formato .NET do Npgsql: Host=...;Database=...;Username=...;Password=...;SSL Mode=VerifyFull;Channel Binding=Require.");
}

var jwtSegredo = builder.Configuration["Jwt:Segredo"];
if (string.IsNullOrWhiteSpace(jwtSegredo) || jwtSegredo.Length < 32)
{
    throw new InvalidOperationException(
        "Configure a variável de ambiente Jwt__Segredo com pelo menos 32 caracteres.");
}

builder.Services.AddDbContext<BrechoDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<AuthServico>();
builder.Services.AddScoped<PecaServico>();
builder.Services.AddScoped<VendaServico>();

builder.Services.AddProblemDetails();
builder.Services.AddControllers();

var origensConfig = builder.Configuration["Cors:Origens"];
var origensPermitidas = string.IsNullOrWhiteSpace(origensConfig)
    ? new[] { "http://localhost:5173" }
    : origensConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("PoliticaCors", policy =>
    {
        policy.WithOrigins(origensPermitidas)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Location");
    });
});

var chaveJwt = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSegredo));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = chaveJwt,
            ValidateIssuer = true,
            ValidIssuer = "Brecho.Api",
            ValidateAudience = true,
            ValidAudience = "Brecho.Cliente",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<BrechoDbContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Garimpo Vintage — API de Brechó",
        Version = "v1",
        Description = "Projeto escolar — empresa fictícia. API REST para gestão de acervo de peças únicas e vendas de brechó."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Cole apenas o token JWT (sem o prefixo 'Bearer ').",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", doc), new List<string>() }
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Brechó API v1");
});

app.UseCors("PoliticaCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.MapHealthChecks("/health");

app.MapGet("/api/versao", (IHostEnvironment env) => Results.Ok(new
{
    commit = Environment.GetEnvironmentVariable("RENDER_GIT_COMMIT") ?? "local",
    ambiente = env.EnvironmentName,
    nicho = "Brechó ou loja de roupas"
}));

app.MapControllers();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var escopo = app.Services.CreateScope();
    var db = escopo.ServiceProvider.GetRequiredService<BrechoDbContext>();
    var adminSenha = builder.Configuration["Seed:AdminSenha"];
    if (string.IsNullOrWhiteSpace(adminSenha))
    {
        throw new InvalidOperationException(
            "Configure a variável de ambiente Seed__AdminSenha para criar a conta de administrador inicial.");
    }

    await db.Database.MigrateAsync();
    await SeedDados.InicializarAsync(db, adminSenha);
}

app.Run();

public partial class Program { }
