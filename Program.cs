using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RestauranteSaborCasero.Data;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// CONEXIÓN A BASE DE DATOS
// ======================================================

string connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'DefaultConnection'."
    );

// ======================================================
// ENTITY FRAMEWORK CORE
// ======================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

// ======================================================
// AUTENTICACIÓN
// ======================================================

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme
)
.AddCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Login/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();

// ======================================================
// MVC
// ======================================================

builder.Services.AddControllersWithViews();

// ======================================================
// RED LOCAL
// ======================================================

// Permite acceder al sistema desde otros dispositivos
// conectados a la misma red.
builder.WebHost.UseUrls("http://0.0.0.0:5133");

var app = builder.Build();

// ======================================================
// BASE DE DATOS
// ======================================================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    try
    {
        Console.WriteLine("Verificando base de datos...");

        // Aplicar migraciones pendientes
        await context.Database.MigrateAsync();

        Console.WriteLine(
            "Base de datos actualizada correctamente."
        );

        // Cargar datos iniciales
        await DbSeeder.SeedAsync(context);

        Console.WriteLine(
            "Datos iniciales cargados correctamente."
        );
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            "========================================"
        );

        Console.WriteLine(
            "ERROR AL INICIALIZAR LA BASE DE DATOS"
        );

        Console.WriteLine(
            "========================================"
        );

        Console.WriteLine(ex.Message);

        if (ex.InnerException != null)
        {
            Console.WriteLine("DETALLE:");
            Console.WriteLine(
                ex.InnerException.Message
            );
        }

        throw;
    }
}

// ======================================================
// PIPELINE
// ======================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// IMPORTANTE:
// No usamos UseHttpsRedirection() mientras probamos
// el acceso HTTP dentro de la red local.

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// ======================================================
// RUTA PRINCIPAL
// ======================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);

app.Run();