using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FixIt.DAL.Data;
using FixIt.DAL.Entities;
using FixIt.DAL.Repositories;
using FixIt.DAL.UnitOfWork;
using FixIt.BLL.Mapping;
using FixIt.BLL.Services;
using FixIt.BLL.Interfaces;
using FixIt.BLL.Validators;
using FixIt.Common.Constants;
using FixIt.Common.Helpers;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// ── Initialize Encryption Helper ──
var encryptionKey = builder.Configuration["EncryptionSettings:Key"];
var encryptionIv = builder.Configuration["EncryptionSettings:IV"];
if (!string.IsNullOrEmpty(encryptionKey) && !string.IsNullOrEmpty(encryptionIv))
{
    EncryptionHelper.Initialize(encryptionKey, encryptionIv);
}

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// ── Database ──
builder.Services.AddDbContext<FixItDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("FixIt.DAL")));

// ── Identity ──
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
    options.Lockout.MaxFailedAccessAttempts = 3;
})
.AddEntityFrameworkStores<FixItDbContext>()
.AddDefaultTokenProviders();

// ── Configuration ──
builder.Services.Configure<FixIt.Common.Settings.SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// ── Cookie Settings ──
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    // Cookie lifetime: 7 days; refreshes on each request while the user is active
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;

    // Harden the cookie
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
});

// Re-validate the security stamp every minute.
// If a user enables/disables 2FA, changes password, or role changes,
// their existing cookie is invalidated within 1 minute.
builder.Services.Configure<Microsoft.AspNetCore.Identity.SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero; // Validate on every request
});

// ── Session ──
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── Repository & Unit of Work ──
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── AutoMapper ──
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// ── Services ──
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddScoped<IIssueService, IssueService>();
builder.Services.AddScoped<IIssueDetailsService, IssueDetailsService>();
builder.Services.AddScoped<IAdminIssueService, AdminIssueService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IRatingAdminService, RatingAdminService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICitizenDashboardService, CitizenDashboardService>();
builder.Services.AddScoped<IEmailSenderService, SmtpEmailSenderService>();

// ── FluentValidation ──
// One registration covers all validators in the same assembly
builder.Services.AddValidatorsFromAssemblyContaining<CreateRatingDtoValidator>();

// ── Authentication (Google) ──
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });

// ── Authorization ──
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
});

var app = builder.Build();

// ── Seed Roles ──
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { AppConstants.CitizenRole, AppConstants.AdminRole };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
