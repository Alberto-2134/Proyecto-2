using EcosistemaAcademico.Models.Data;
using EcosistemaAcademico.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcosistemaAcademico.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class EstudiantesController : Controller
    {
        private readonly AppDbContext _context;
        public EstudiantesController(AppDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.Estudiantes.OrderBy(e => e.Apellido).ToListAsync());

        public async Task<IActionResult> Formulario(int? id)
        {
            var model = id == null ? new Estudiante() : await _context.Estudiantes.FindAsync(id);
            if (model == null) return NotFound();
            return PartialView("_Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(Estudiante model, string? password)
        {
            // Estos dos los asigna el sistema, no el formulario
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Rol");

            if (model.Id == 0 && string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("", "La contraseña es obligatoria al crear");

            if (await _context.Usuarios.AnyAsync(u => u.Correo == model.Correo && u.Id != model.Id))
                ModelState.AddModelError("Correo", "Ese correo ya está registrado");

            if (!ModelState.IsValid) return PartialView("_Form", model);

            if (model.Id == 0)
            {
                model.Rol = "Estudiante";
                model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                model.FechaRegistro = DateTime.Now;
                _context.Estudiantes.Add(model);
            }
            else
            {
                var actual = await _context.Estudiantes.FindAsync(model.Id);
                if (actual == null) return NotFound();

                actual.Nombre = model.Nombre;
                actual.Apellido = model.Apellido;
                actual.Correo = model.Correo;
                actual.Estado = model.Estado;
                actual.Matricula = model.Matricula;
                actual.Carrera = model.Carrera;
                actual.Nivel = model.Nivel;
                actual.Promedio = model.Promedio;
                actual.NecesitaIntervencion = model.NecesitaIntervencion;

                if (!string.IsNullOrWhiteSpace(password))
                    actual.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            }

            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var item = await _context.Estudiantes.FindAsync(id);
            if (item != null)
            {
                try
                {
                    _context.Estudiantes.Remove(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "No se puede eliminar: el estudiante tiene diagnósticos o emparejamientos.";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
