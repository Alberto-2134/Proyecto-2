using System.Security.Claims;
using EcosistemaAcademico.Models;
using EcosistemaAcademico.Models.Data;
using EcosistemaAcademico.Models.Entidades;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcosistemaAcademico.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        public AccountController(AppDbContext context) { _context = context; }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == model.Correo);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos");
                return View(model);
            }

            // Primero se verifica la contraseña, y solo entonces se revela el estado de la cuenta
            if (!usuario.Estado)
            {
                ModelState.AddModelError("", "Tu cuenta está inactiva o pendiente de aprobación");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre + " " + usuario.Apellido),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Por seguridad, nunca se puede registrar un administrador desde aquí
            if (model.Rol != "Estudiante" && model.Rol != "Tutor")
                ModelState.AddModelError("Rol", "Rol no válido");

            if (model.Rol == "Estudiante")
            {
                if (string.IsNullOrWhiteSpace(model.Matricula))
                    ModelState.AddModelError("Matricula", "La matrícula es obligatoria");
                if (string.IsNullOrWhiteSpace(model.Carrera))
                    ModelState.AddModelError("Carrera", "La carrera es obligatoria");
            }
            else if (model.Rol == "Tutor")
            {
                if (string.IsNullOrWhiteSpace(model.Especialidad))
                    ModelState.AddModelError("Especialidad", "La especialidad es obligatoria");
                if (string.IsNullOrWhiteSpace(model.Disponibilidad))
                    ModelState.AddModelError("Disponibilidad", "La disponibilidad es obligatoria");
            }

            if (await _context.Usuarios.AnyAsync(u => u.Correo == model.Correo))
                ModelState.AddModelError("Correo", "Ese correo ya está registrado");

            if (!ModelState.IsValid) return View(model);

            var hash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            if (model.Rol == "Estudiante")
            {
                _context.Estudiantes.Add(new Estudiante
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Correo = model.Correo,
                    PasswordHash = hash,
                    Rol = "Estudiante",
                    FechaRegistro = DateTime.Now,
                    Estado = true,
                    Matricula = model.Matricula!,
                    Carrera = model.Carrera!,
                    Nivel = model.Nivel,
                    Promedio = 0,
                    NecesitaIntervencion = false
                });
            }
            else
            {
                _context.Tutores.Add(new Tutor
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Correo = model.Correo,
                    PasswordHash = hash,
                    Rol = "Tutor",
                    FechaRegistro = DateTime.Now,
                    Estado = false,                 // pendiente de aprobación
                    Especialidad = model.Especialidad!,
                    Disponibilidad = model.Disponibilidad!,
                    CalificacionPromedio = 0,
                    CapacidadMaxima = 3
                });
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = model.Rol == "Tutor"
                ? "Cuenta creada. Un administrador debe activarla antes de que puedas entrar."
                : "Cuenta creada. Ya puedes iniciar sesión.";
            return RedirectToAction("Login");
        }

        public IActionResult AccesoDenegado() => View();

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}