using EcosistemaAcademico.Models.Data;
using EcosistemaAcademico.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace EcosistemaAcademico.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministradoresController : Controller
    {
        private readonly AppDbContext _context;
        public AdministradoresController(AppDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.Administradores.OrderBy(t => t.Apellido).ToListAsync());

        public async Task<IActionResult> Formulario(int? id)
        {
            var model = id == null ? new Administrador() : await _context.Administradores.FindAsync(id);
            if (model == null) return NotFound();
            return PartialView("_Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(Administrador model, string? password)
        {
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Rol");

            if (model.Id == 0 && string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("", "La contraseña es obligatoria al crear");

            if (await _context.Usuarios.AnyAsync(u => u.Correo == model.Correo && u.Id != model.Id))
                ModelState.AddModelError("Correo", "Ese correo ya está registrado");

            if (!ModelState.IsValid) return PartialView("_Form", model);

            if (model.Id == 0)
            {
                model.Rol = "Administrador";
                model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                model.FechaRegistro = DateTime.Now;
                _context.Administradores.Add(model);
            }
            else
            {
                var actual = await _context.Administradores.FindAsync(model.Id);
                if (actual == null) return NotFound();

                actual.Nombre = model.Nombre;
                actual.Apellido = model.Apellido;
                actual.Correo = model.Correo;
                actual.Estado = model.Estado;
                actual.Departamento = model.Departamento;
                actual.NivelAcceso = model.NivelAcceso;
                actual.Cargo = model.Cargo;

                if (!string.IsNullOrWhiteSpace(password))
                    actual.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            }

            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var miId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (id == miId)
            {
                TempData["Error"] = "No puedes eliminar tu propio usuario.";
                return RedirectToAction(nameof(Index));
            }

            var item = await _context.Administradores.FindAsync(id);
            if (item != null)
            {
                try
                {
                    _context.Administradores.Remove(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "No se puede eliminar: tiene repositorios asignados.";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}