using ChatApp.Endpoint.Data;
using ChatApp.Endpoint.Helpers;
using ChatApp.Endpoint.Hubs;     // A mi �j hub-unk
using ChatApp.Endpoint.Services; // A mi �j service-�nk
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

// Filterek regisztr�l�sa (hiba- �s valid�ci�kezel�s)
builder.Services.AddControllers(opt =>
{
    opt.Filters.Add<ExceptionFilter>();
    opt.Filters.Add<ValidationFilter>();
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Swagger/OpenAPI be�ll�t�sa a tesztel�shez
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

// ----- SAJ�T SZOLG�LTAT�SOK REGISZTR�L�SA -----
// ChatHistoryService regisztr�l�sa Singleton-k�nt, hogy egyetlen p�ld�ny legyen bel�le.
builder.Services.AddSingleton<ChatHistoryService>();
builder.Services.AddTransient<ChatHub>();


// ----- IDENTITY �s ADATB�ZIS BE�LL�T�SA -----
// Az Identity-hez kell egy DbContext, m�g ha az �zeneteket nem is itt t�roljuk.
builder.Services.AddDbContext<ChatAppContext>(opt =>
{
    opt.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ChatAppDb;Trusted_Connection=True;TrustServerCertificate=True;");
});

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ChatAppContext>()
    .AddDefaultTokenProviders();


// ----- AUTHENTIK�CI� (JWT) -----
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Developmenthez k�nnyebb false-on hagyni
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = "chatapp.com",
        ValidIssuer = "chatapp.com",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwt:key"] ?? throw new Exception("jwt:key not found")))
    };
});


// ----- SIGNALR �s HANGFIRE -----
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

// SignalR Hub v�gpont be�ll�t�sa
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chathub");
});

// Hangfire Dashboard enged�lyez�se
app.UseHangfireDashboard();


// ----- ALKALMAZ�S IND�T�SAKOR LEFUT� LOGIKA �S HANGFIRE JOB BE�LL�T�SA -----
using (var scope = app.Services.CreateScope())
{
    // 1. Singleton service lek�r�se
    var chatHistoryService = scope.ServiceProvider.GetRequiredService<ChatHistoryService>();

    // 2. Chat el�zm�nyek bet�lt�se a JSON f�jlb�l az alkalmaz�s indul�sakor
    chatHistoryService.LoadFromFile();

    // 3. Ism�tl�d� Hangfire job be�ll�t�sa, ami percenk�nt elmenti a chatet
    RecurringJob.AddOrUpdate(
        "save-chat-to-json-job",                      // A job egyedi azonos�t�ja
        () => chatHistoryService.SaveToFile(),        // A met�dus, amit meg kell h�vni
        Cron.Minutely()                               // Milyen gyakran (percenk�nt)
    );
}

app.Run();