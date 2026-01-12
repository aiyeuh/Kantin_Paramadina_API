using Microsoft.EntityFrameworkCore;
using Kantin_Paramadina.Model;
using System.Text.Json.Serialization;
using Kantin_Paramadina.Mappings;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Kantin_Paramadina.Middleware;
using System.IdentityModel.Tokens.Jwt;
using Kantin_Paramadina.Hubs;

var builder = WebApplication.CreateBuilder(args);
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
//Connection String ke SQL Server lokal
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=HP-Fakhri211000\\SQLEXPRESS;Database=KantinDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ───── JWT setup ─────
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // <-- Masukkan URL React kamu di sini!
                  .AllowAnyHeader()
                  .AllowAnyMethod(); // Ini yang mengizinkan method OPTIONS, GET, POST, PUT, dll
        });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            NameClaimType = "username",
            RoleClaimType = "role"
        };

        // For SignalR authentication over WebSockets
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/transactionHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Add Midtrans (Payment Gateway) HttpClient
builder.Services.AddHttpClient<MidtransSnapService>();

// Add SignalR
builder.Services.AddSignalR();

// ───── AutoMapper & Swagger ─────
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Kantin Paramadina API", Version = "v1" });
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Masukkan token Bearer Anda.",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// ───── Seeder Admin/Customer default ─────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// ───── Pipeline ─────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // 🔹 Serve wwwroot/ untuk gambar menu, qris, dll
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseMiddleware<TokenRevocationMiddleware>(); // cek blacklist token
app.UseAuthorization();

app.MapControllers();
app.MapHub<TransactionHub>("/transactionHub");
app.Run();