using EcosistemaAcademico.Models.Data;
using EcosistemaAcademico.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcosistemaAcademico.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SeguimientosController : Controller
    {
        private readonly AppDbContext _context;
        public SeguimientosController(AppDbContext context) { _context = context; }

        // Lista desplegable: "Estudiante | Tutor: Nombre | CÓDIGO"
        private async Task CargarListas()
        {
            var emparejamientos = (await _context.Emparejamientos
                    .Include(e => e.Estudiante)
                    .Include(e => e.Tutor)
                    .Include(e => e.Asignatura)
                    .Where(e => e.Estado != "Cancelado")
                    .ToListAsync())
                .Select(e => new
                {
                    e.Id,
                    Nombre = $"{e.Estudiante?.Nombre} {e.Estudiante?.Apellido} | Tutor: {e.Tutor?.Nombre} {e.Tutor?.Apellido} | {e.Asignatura?.Codigo}"
                });

            ViewBag.Emparejamientos = new SelectList(emparejamientos, "Id", "Nombre");
        }

        // Actualiza el estado del emparejamiento según su mayor progreso
        private async Task ActualizarEstadoEmparejamiento(int emparejamientoId)
        {
            var emp = await _context.Emparejamientos.FindAsync(emparejamientoId);
            if (emp == null || emp.Estado == "Cancelado") return;

            var seguimientos = _context.Seguimientos.Where(s => s.EmparejamientoId == emparejamientoId);
            if (!await seguimientos.AnyAsync()) return;

            var maxProgreso = await seguimientos.MaxAsync(s => s.Progreso);
            emp.Estado = maxProgreso >= 100 ? "Finalizado" : "Activo";
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> Index()
            => View(await _context.Seguimientos
                .Include(s => s.Emparejamiento).ThenInclude(e => e!.Estudiante)
                .Include(s => s.Emparejamiento).ThenInclude(e => e!.Tutor)
                .Include(s => s.Emparejamiento).ThenInclude(e => e!.Asignatura)
                .OrderByDescending(s => s.Fecha)
                .ToListAsync());

        public async Task<IActionResult> Formulario(int? id)
        {
            var model = id == null ? new SeguimientoAcademico() : await _context.Seguimientos.FindAsync(id);
            if (model == null) return NotFound();
            await CargarListas();
            return PartialView("_Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(SeguimientoAcademico model)
        {
            if (model.Progreso < 0 || model.Progreso > 100)
                ModelState.AddModelError("Progreso", "El progreso debe estar entre 0 y 100");

            if (model.CalificacionAntes < 0 || model.CalificacionAntes > 100 ||
                model.CalificacionDespues < 0 || model.CalificacionDespues > 100)
                ModelState.AddModelError("", "Las calificaciones deben estar entre 0 y 100");

            if (model.ProximaSesion != null && model.ProximaSesion < model.Fecha)
                ModelState.AddModelError("ProximaSesion", "La próxima sesión no puede ser antes de la fecha del seguimiento");

            if (!ModelState.IsValid)
            {
                await CargarListas();
                return PartialView("_Form", model);
            }

            if (model.Id == 0) _context.Seguimientos.Add(model);
            else _context.Seguimientos.Update(model);

            await _context.SaveChangesAsync();
            await ActualizarEstadoEmparejamiento(model.EmparejamientoId);
            return Json(new { ok = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var item = await _context.Seguimientos.FindAsync(id);
            if (item != null)
            {
                var empId = item.EmparejamientoId;
                _context.Seguimientos.Remove(item);
                await _context.SaveChangesAsync();
                await ActualizarEstadoEmparejamiento(empId);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
