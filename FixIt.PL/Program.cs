using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FixIt.DAL.Data;
using FixIt.DAL.Entities;
using FixIt.DAL.Repositories;
using FixIt.DAL.UnitOfWork;
using FixIt.BLL.Mapping;
using FixIt.BLL.Interfaces;
using FixIt.BLL.Validators;
using FixIt.Common.Constants;
using FluentValidation;
using FixIt.BLL.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ── Database ──
builder.Services.AddDbContext<FixItDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// ── Repository & Unit of Work ──
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── AutoMapper ──
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ── FluentValidation ──
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

// ── Account Service (M1) ──
builder.Services.AddScoped<IAccountService, AccountService>();

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

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
