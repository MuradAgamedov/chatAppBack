using chatApp.Data;
using chatApp.Hubs; // ⬅️ Əlavə et: SignalR Hub üçün
using chatApp.Interfaces;
using chatApp.Models;
using chatApp.Repositories;
using chatApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// 🔥 CORS siyasətini təyin et
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // React frontend URL-inə icazə veririk
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // 👈 Token və cookie ötürülməsinə icazə ver
        });
});

// 🔥 Verilənlər bazasını əlavə et
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 🔥 Identity sistemi
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 🔥 Servislər
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IMessageService, MessageService>();


// 🔥 Authentication və JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
        ClockSkew = TimeSpan.Zero
    };

    // 👇 WebSocket üzərindən authentication üçün
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// 🔥 SignalR əlavə et
builder.Services.AddSignalR();

// 🔥 Kontrollerlər və Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔥 Static fayllar üçün
builder.Services.AddDirectoryBrowser();

var app = builder.Build();

// 🔥 Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔥 CORS
app.UseCors(MyAllowSpecificOrigins);

// 🔥 Auth və static
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

// 🔥 Kontrollerlər və SignalR hub
app.MapControllers();
app.MapHub<MessageHub>("/hubs/chat");

app.Run();
