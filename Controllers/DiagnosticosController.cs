using EcosistemaAcademico.Models.Data;
using EcosistemaAcademico.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcosistemaAcademico.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class DiagnosticosController : Controller
    {
        private readonly AppDbContext _context;
        public DiagnosticosController(AppDbContext context) { _context = context; }

        private async Task CargarListas()
        {
            var estudiantes = await _context.Estudiantes
                .Select(e => new { e.Id, Nombre = e.Matricula + " - " + e.Nombre + " " + e.Apellido })
                .ToListAsync();
            var asignaturas = await _context.Asignaturas
                .Select(a => new { a.Id, Nombre = a.Codigo + " - " + a.Nombre })
                .ToListAsync();

            ViewBag.Estudiantes = new SelectList(estudiantes, "Id", "Nombre");
            ViewBag.Asignaturas = new SelectList(asignaturas, "Id", "Nombre");
        }

        // Un estudiante necesita intervención si tiene algún diagnóstico en riesgo Alto
        private async Task ActualizarIntervencion(int estudianteId)
        {
            var estudiante = await _context.Estudiantes.FindAsync(estudianteId);
            if (estudiante == null) return;

            estudiante.NecesitaIntervencion = await _context.Diagnosticos
                .AnyAsync(d => d.EstudianteId == estudianteId && d.NivelRiesgo == "Alto");
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> Index()
            => View(await _context.Diagnosticos
                .Include(d => d.Estudiante)
                .Include(d => d.Asignatura)
                .OrderByDescending(d => d.Fecha)
                .ToListAsync());

        public async Task<IActionResult> Formulario(int? id)
        {
            var model = id == null ? new DiagnosticoAcademico() : await _context.Diagnosticos.FindAsync(id);
            if (model == null) return NotFound();
            await CargarListas();
            return PartialView("_Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(DiagnosticoAcademico model)
        {
            if (!ModelState.IsValid)
            {
                await CargarListas();
                return PartialView("_Form", model);
            }

            if (model.Id == 0) _context.Diagnosticos.Add(model);
            else _context.Diagnosticos.Update(model);

            await _context.SaveChangesAsync();
            await ActualizarIntervencion(model.EstudianteId);
            return Json(new { ok = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var item = await _context.Diagnosticos.FindAsync(id);
            if (item != null)
            {
                var estudianteId = item.EstudianteId;
                _context.Diagnosticos.Remove(item);
                await _context.SaveChangesAsync();
                await ActualizarIntervencion(estudianteId);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
