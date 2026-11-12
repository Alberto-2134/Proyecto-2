using EcosistemaAcademico.Models.Data;
using EcosistemaAcademico.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcosistemaAcademico.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class TutorAsignaturasController : Controller
    {
        private readonly AppDbContext _context;
        public TutorAsignaturasController(AppDbContext context) { _context = context; }

        private async Task CargarListas()
        {
            var tutores = await _context.Tutores
                .Select(t => new { t.Id, Nombre = t.Nombre + " " + t.Apellido })
                .ToListAsync();
            var asignaturas = await _context.Asignaturas
                .Select(a => new { a.Id, Nombre = a.Codigo + " - " + a.Nombre })
                .ToListAsync();

            ViewBag.Tutores = new SelectList(tutores, "Id", "Nombre");
            ViewBag.Asignaturas = new SelectList(asignaturas, "Id", "Nombre");
        }

        public async Task<IActionResult> Index()
            => View(await _context.TutorAsignaturas
                .Include(x => x.Tutor)
                .Include(x => x.Asignatura)
                .OrderBy(x => x.Tutor!.Apellido)
                .ToListAsync());

        public async Task<IActionResult> Formulario(int? id)
        {
            var model = id == null ? new TutorAsignatura() : await _context.TutorAsignaturas.FindAsync(id);
            if (model == null) return NotFound();
            await CargarListas();
            return PartialView("_Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(TutorAsignatura model)
        {
            // Un tutor no puede tener la misma asignatura dos veces
            if (await _context.TutorAsignaturas.AnyAsync(x =>
                    x.TutorId == model.TutorId &&
                    x.AsignaturaId == model.AsignaturaId &&
                    x.Id != model.Id))
                ModelState.AddModelError("", "Ese tutor ya tiene asignada esa asignatura");

            if (!ModelState.IsValid)
            {
                await CargarListas();
                return PartialView("_Form", model);
            }

            if (model.Id == 0) _context.TutorAsignaturas.Add(model);
            else _context.TutorAsignaturas.Update(model);

            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var item = await _context.TutorAsignaturas.FindAsync(id);
            if (item != null)
            {
                _context.TutorAsignaturas.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
