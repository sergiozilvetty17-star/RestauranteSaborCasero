using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CONEXIÓN A MYSQL
// ==========================================

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);


// ==========================================
// AUTENTICACIÓN
// ==========================================

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme
)
.AddCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Login/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});


// ==========================================
// AUTORIZACIÓN
// ==========================================

builder.Services.AddAuthorization();


// ==========================================
// MVC
// ==========================================

builder.Services.AddControllersWithViews();


var app = builder.Build();


// ==========================================
// CONFIGURACIÓN HTTP
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();


// ==========================================
// ROUTING
// ==========================================

app.UseRouting();


// ==========================================
// AUTENTICACIÓN Y AUTORIZACIÓN
// ==========================================

app.UseAuthentication();

app.UseAuthorization();


// ==========================================
// RUTA PRINCIPAL
// ==========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);


app.Run();