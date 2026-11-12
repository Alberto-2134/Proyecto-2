using EcosistemaAcademico.Models.Data;
using EcosistemaAcademico.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcosistemaAcademico.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class TutoresController : Controller
    {
        private readonly AppDbContext _context;
        public TutoresController(AppDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.Tutores.OrderBy(t => t.Apellido).ToListAsync());

        public async Task<IActionResult> Formulario(int? id)
        {
            var model = id == null ? new Tutor() : await _context.Tutores.FindAsync(id);
            if (model == null) return NotFound();
            return PartialView("_Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(Tutor model, string? password)
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
                model.Rol = "Tutor";
                model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                model.FechaRegistro = DateTime.Now;
                _context.Tutores.Add(model);
            }
            else
            {
                var actual = await _context.Tutores.FindAsync(model.Id);
                if (actual == null) return NotFound();

                actual.Nombre = model.Nombre;
                actual.Apellido = model.Apellido;
                actual.Correo = model.Correo;
                actual.Estado = model.Estado;
                actual.Especialidad = model.Especialidad;
                actual.Disponibilidad = model.Disponibilidad;
                actual.CalificacionPromedio = model.CalificacionPromedio;
                actual.CapacidadMaxima = model.CapacidadMaxima;

                if (!string.IsNullOrWhiteSpace(password))
                    actual.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            }

            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var item = await _context.Tutores.FindAsync(id);
            if (item != null)
            {
                try
                {
                    _context.Tutores.Remove(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "No se puede eliminar: el tutor tiene asignaturas, emparejamientos o recursos.";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}