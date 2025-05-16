using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ServiPuntos.Infrastructure.Data;
using ServiPuntos.Infrastructure.MultiTenancy;
using ServiPuntos.Core.Interfaces;
using ServiPuntos.Infrastructure.Repositories;
using ServiPuntos.Infrastructure.Middleware;
using System.Text;
using System.Security.Claims;

// Creaci�n de la aplicaci�n web ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Trace);

// Agregar controladores a la aplicaci�n
builder.Services.AddControllersWithViews();

// Agregar servicios de Swagger para documentaci�n de API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuraci�n JWT (JSON Web Token)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

// A�ade esto en Program.cs antes de construir la aplicaci�n
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "keys")))
    .SetApplicationName("ServiPuntos");
    //.ProtectKeysWithDpapi();

//Soporte de sesion
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Para HTTPS
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Configurar servicios de autenticaci�n
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Ya no necesitas DefaultChallengeScheme para Google
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.Path = "/";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
    options.Cookie.IsEssential = true;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

//Con esto permitimos solicitudes desde el frontend-web
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins("http://localhost:3000") // Frontend HTTP
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials() // Importante para enviar cookies
            .SetIsOriginAllowed(_ => true));
});


// Agregar el servicio JwtTokenService al contenedor de dependencias
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddHttpClient();
// Servicios Multi-Tenant
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantResolver, TenantResolver>();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Configuramos la conexi�n a la base de datos
builder.Services.AddDbContext<TenantConfigurationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<ServiPuntosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Construye la aplicaci�n web
var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP (middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // Importante para asegurar HTTPS
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowReactApp");
app.UseSession(); 
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});


app.Run();