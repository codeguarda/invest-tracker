using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using InvestTracker.Application.Investments;
using InvestTracker.Domain.Users;
using InvestTracker.Infrastructure.Mongo;
using InvestTracker.Infrastructure.Persistence;
using InvestTracker.Infrastructure.Queries;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;        //necessário para Swagger c/ Bearer
using MongoDB.Driver;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
builder.Logging.AddConsole();

builder.Services.AddDbContext<AppWriteDbContext>(o =>
{
    o.UseNpgsql(config.GetConnectionString("Postgres"));
    o.EnableSensitiveDataLogging(); // vai logar valores dos parâmetros do INSERT
});

// ===== CORS (Vite/React) =====
var allowedOrigins = new[]
{
    "http://localhost:5173",
    "http://127.0.0.1:5173",
    "http://10.0.2.15:5173",
    "http://172.18.0.1:5173"
};

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("DevCors", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
    );
});

// ===== Auth (JWT) =====
var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    // mantém os nomes das claims (ex.: "sub" continua "sub")
    o.MapInboundClaims = false;

    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
builder.Services.AddAuthorization();

// ===== MediatR + AutoMapper =====
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateInvestmentHandler).Assembly));
builder.Services.AddAutoMapper(typeof(CreateInvestmentHandler).Assembly);

// ===== EF Core (Postgres) =====
builder.Services.AddDbContext<AppWriteDbContext>(o =>
    o.UseNpgsql(config.GetConnectionString("Postgres")));

// ===== Dapper connection =====
builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(config.GetConnectionString("Postgres")));
builder.Services.AddScoped<InvestmentSqlQueries>();

// ===== Mongo (IMongoClient + IMongoDatabase + MongoContext) =====
builder.Services.AddSingleton<IMongoClient>(_ =>
{
    var cs = config["Mongo:ConnectionString"] ?? "mongodb://localhost:27017";
    return new MongoClient(cs);
});
builder.Services.AddSingleton(sp =>
{
    var dbName = config["Mongo:Database"] ?? "investread";
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(dbName);
});
builder.Services.AddSingleton<MongoContext>();

// ===== Swagger (com JWT Bearer) =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "InvestTracker API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe: Bearer {seu_token}"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// CORS antes de auth
app.UseCors("DevCors");

app.UseAuthentication();
app.UseAuthorization();

// ===== Seed demo user =====
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppWriteDbContext>();
//     await db.Database.MigrateAsync();
//     if (!await db.Users.AnyAsync())
//     {
//         var demo = User.Create("demo@local", BCrypt.Net.BCrypt.HashPassword("demo123"));
//         db.Users.Add(demo);
//         await db.SaveChangesAsync();
//         Console.WriteLine($"Seeded demo user: demo@local / demo123 (Id: {demo.Id})");
//     }
// }

// Auth - Register
app.MapPost("/api/auth/register", async (LoginRequest req, AppWriteDbContext db) =>
{
    var email = req.Email.Trim().ToLowerInvariant();
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "Email e senha obrigatórios." });

    var exists = await db.Users.AnyAsync(u => u.Email == email);
    if (exists) return Results.Conflict(new { error = "Email já cadastrado." });

    var user = User.Create(email, BCrypt.Net.BCrypt.HashPassword(req.Password));
    db.Users.Add(user);
    await db.SaveChangesAsync();

    // Gera o mesmo JWT do login
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(ClaimTypes.NameIdentifier,     user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email)
    };
    var token = new JwtSecurityToken(
        issuer:  config["Jwt:Issuer"],
        audience: config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
    );
    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Created("/api/auth/register", new { token = jwt });
});

// Helper p/ extrair Guid do token
static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
{
    var idStr =
        user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
        user.FindFirstValue(ClaimTypes.NameIdentifier);

    return Guid.TryParse(idStr, out userId);
}

// ===== Endpoints =====

// Auth
app.MapPost("/api/auth/login", async (LoginRequest req, AppWriteDbContext db) =>
{
    var email = req.Email.Trim().ToLowerInvariant();
    var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
    if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        return Results.Unauthorized();

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(ClaimTypes.NameIdentifier,     user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email)
    };

    var token = new JwtSecurityToken(
        issuer:  config["Jwt:Issuer"],
        audience: config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
    );

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { token = jwt });
});

// Investments - Create
// Investments - Create
app.MapPost("/api/investments", async (CreateInvestmentBody body, IMediator m, ClaimsPrincipal user) =>
{
    if (!TryGetUserId(user, out var userId))
        return Results.Unauthorized();

    if (userId == Guid.Empty)
        return Results.BadRequest("UserId empty in token/claims.");

    var id = await m.Send(new CreateInvestmentCommand(
        userId,
        body.Type,
        body.Amount,
        DateOnly.Parse(body.Date),
        body.Description));

    // devolvemos o userId usado, para conferência rápida no Swagger
    return Results.Created($"/api/investments/{id}", new { id, userIdUsed = userId });
}).RequireAuthorization();

// Investments - List (Dapper/Postgres)
app.MapGet("/api/investments", async (int page, int size, ClaimsPrincipal user, InvestmentSqlQueries q) =>
{
    if (!TryGetUserId(user, out var userId)) return Results.Unauthorized();

    var take = Math.Clamp(size, 1, 100);
    var skip = Math.Max(0, (page - 1) * take);
    var data = await q.ListByUserAsync(userId, skip, take);
    return Results.Ok(data);
}).RequireAuthorization();

// Dashboard (Mongo)
app.MapGet("/api/dashboard", async (string? from, string? to, ClaimsPrincipal user, IMediator m) =>
{
    if (!TryGetUserId(user, out var userId)) return Results.Unauthorized();

    DateOnly? f = string.IsNullOrWhiteSpace(from) ? null : DateOnly.Parse(from);
    DateOnly? t = string.IsNullOrWhiteSpace(to) ? null : DateOnly.Parse(to);

    var dto = await m.Send(new GetDashboardQuery(userId, f, t));
    return Results.Ok(dto);
}).RequireAuthorization();

app.Run();

// Records
public sealed record LoginRequest(string Email, string Password);
public sealed record CreateInvestmentBody(string Type, decimal Amount, string Date, string? Description);
