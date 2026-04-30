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

// ── Database ──
builder.Services.AddDbContext<FixItDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("FixIt.DAL")));

// ── Identity ──
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<FixItDbContext>()
.AddDefaultTokenProviders();

// ── Cookie Settings ──
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
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
builder.Services.AddScoped<IRatingService, RatingService>();

// ── FluentValidation ──
// One registration covers all validators in the same assembly
builder.Services.AddValidatorsFromAssemblyContaining<CreateRatingDtoValidator>();

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
