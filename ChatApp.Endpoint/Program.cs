using ChatApp.Endpoint.Data;
using ChatApp.Endpoint.Helpers;
using ChatApp.Endpoint.Hubs;     // A mi új hub-unk
using ChatApp.Endpoint.Services; // A mi új service-ünk
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Filterek regisztrálása (hiba- és validációkezelés)
builder.Services.AddControllers(opt =>
{
    opt.Filters.Add<ExceptionFilter>();
    opt.Filters.Add<ValidationFilter>();
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Swagger/OpenAPI beállítása a teszteléshez
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatApp API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

// ----- SAJÁT SZOLGÁLTATÁSOK REGISZTRÁLÁSA -----
// ChatHistoryService regisztrálása Singleton-ként, hogy egyetlen példány legyen belõle.
builder.Services.AddSingleton<ChatHistoryService>();
builder.Services.AddTransient<ChatHub>();


// ----- IDENTITY és ADATBÁZIS BEÁLLÍTÁSA -----
// Az Identity-hez kell egy DbContext, még ha az üzeneteket nem is itt tároljuk.
builder.Services.AddDbContext<ChatAppContext>(opt =>
{
    opt.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ChatAppDb;Trusted_Connection=True;TrustServerCertificate=True;");
});

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ChatAppContext>()
    .AddDefaultTokenProviders();


// ----- AUTHENTIKÁCIÓ (JWT) -----
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Developmenthez könnyebb false-on hagyni
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = "chatapp.com",
        ValidIssuer = "chatapp.com",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwt:key"] ?? throw new Exception("jwt:key not found")))
    };
});


// ----- SIGNALR és HANGFIRE -----
builder.Services.AddSignalR();
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage("Server=(localdb)\\MSSQLLocalDB;Database=ChatAppDb;Trusted_Connection=True;TrustServerCertificate=True;"));
builder.Services.AddHangfireServer();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Fontos a sorrend: Authentication -> Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR Hub végpont beállítása
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chathub");
});

// Hangfire Dashboard engedélyezése
app.UseHangfireDashboard();


// ----- ALKALMAZÁS INDÍTÁSAKOR LEFUTÓ LOGIKA ÉS HANGFIRE JOB BEÁLLÍTÁSA -----
using (var scope = app.Services.CreateScope())
{
    // 1. Singleton service lekérése
    var chatHistoryService = scope.ServiceProvider.GetRequiredService<ChatHistoryService>();

    // 2. Chat elõzmények betöltése a JSON fájlból az alkalmazás indulásakor
    chatHistoryService.LoadFromFile();

    // 3. Ismétlõdõ Hangfire job beállítása, ami percenként elmenti a chatet
    RecurringJob.AddOrUpdate(
        "save-chat-to-json-job",                      // A job egyedi azonosítója
        () => chatHistoryService.SaveToFile(),        // A metódus, amit meg kell hívni
        Cron.Minutely()                               // Milyen gyakran (percenként)
    );
}

app.Run();