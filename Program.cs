using System.Security.Claims;
using System.Text;
using DemoAuth.Data;
using DemoAuth.Interface;
using DemoAuth.Models.Accounts;
using DemoAuth.Repository.Accounts;
using DemoAuth.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// It's a good practice to add services before adding middleware and to keep related configurations grouped together. For clarity and better structure
// Register the interface and its implementation (SERVICES)
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
// Email sender for email confirmation
builder.Services.AddSingleton<IEmailSender, EmailSender>();


// (SERVICES)
// Register Identity with custom User class (ApplicationUser)
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true; // Enforces email confirmation
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// Register DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);


// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;  // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
        ),
        // RoleClaimType = "role"
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        // RoleClaimType = ClaimTypes.Role // <-- This tells the middleware to use the default role claim type
        // Because default roleclaimtype when creating a token using ClaimTypes.Role is http://schemas.microsoft.com/ws/2008/06/identity/claims/role
    };
});

// Add Authorization
builder.Services.AddAuthorization();


// Add controllers and configure JSON serialization
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});
// no need for this one if above is implemented
// builder.Services.AddControllers();





var app = builder.Build();

app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization();  // Enable authorization middleware

app.MapControllers();

app.Run();
