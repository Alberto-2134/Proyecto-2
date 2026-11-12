using EcosistemaAcademico.Models.Data;
using EcosistemaAcademico.Models.Entidades;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/AccesoDenegado";
    });

var app = builder.Build();

// Administrador inicial y datos de ejemplo
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Administradores.Any())
    {
        db.Administradores.Add(new Administrador
        {
            Nombre = "Admin",
            Apellido = "Sistema",
            Correo = "admin@ecosistema.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*"),
            Rol = "Administrador",
            Departamento = "Tecnología",
            NivelAcceso = "Total",
            Cargo = "Administrador general"
        });
        db.SaveChanges();
    }

    // Datos de ejemplo para la demostración (solo en desarrollo)
    if (app.Environment.IsDevelopment())
        DatosDemo.Sembrar(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();