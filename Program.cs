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

// 1. Setup Dasar
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

// Connection String ke SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=HP-Fakhri211000\\SQLEXPRESS;Database=KantinDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Setup JWT Settings
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

// 3. Setup CORS (Policy Definition)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins(
                    "https://kantin.jackserver.site", // Domain Production
                    "http://localhost:5173",          // Vite Local
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Penting jika pakai SignalR atau Cookie
        });
});

// 4. Setup Authentication
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

        // Konfigurasi khusus untuk SignalR via WebSockets
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

// 5. Services Lainnya
builder.Services.AddHttpClient<MidtransSnapService>();
builder.Services.AddSignalR();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 6. Setup Swagger
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

// ───── Seeder Database (Opsional: Hati-hati di Production) ─────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // db.Database.Migrate(); // Uncomment jika ingin auto-migrate
}

// =========================================================
// PIPELINE MIDDLEWARE (URUTAN SANGAT PENTING!)
// =========================================================

// 1. Swagger (Development Only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. Redirection & Files
app.UseHttpsRedirection();
app.UseStaticFiles(); // Load file gambar/asset

// 3. Routing (WAJIB DITAMBAHKAN SEBELUM CORS)
app.UseRouting();

// 4. CORS (WAJIB SESUDAH ROUTING, SEBELUM AUTH)
app.UseCors("AllowReactApp");

// 5. Authentication & Authorization
app.UseAuthentication();
app.UseMiddleware<TokenRevocationMiddleware>(); // Middleware kustom
app.UseAuthorization();

// 6. Endpoints
app.MapControllers();
app.MapHub<TransactionHub>("/transactionHub");

app.Run();