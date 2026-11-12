using EcosistemaAcademico.Models.Data;
using EcosistemaAcademico.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcosistemaAcademico.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RecursosController : Controller
    {
        private readonly AppDbContext _context;
        public RecursosController(AppDbContext context) { _context = context; }

        private async Task CargarListas()
        {
            var asignaturas = await _context.Asignaturas
                .Select(a => new { a.Id, Nombre = a.Codigo + " - " + a.Nombre })
                .ToListAsync();
            var repositorios = await _context.Repositorios
                .Select(r => new { r.Id, Nombre = r.Nombre + " (" + r.Sede + ")" })
                .ToListAsync();
            var tutores = await _context.Tutores
                .Select(t => new { t.Id, Nombre = t.Nombre + " " + t.Apellido })
                .ToListAsync();

            ViewBag.Asignaturas = new SelectList(asignaturas, "Id", "Nombre");
            ViewBag.Repositorios = new SelectList(repositorios, "Id", "Nombre");
            ViewBag.Tutores = new SelectList(tutores, "Id", "Nombre");
        }

        public async Task<IActionResult> Index()
            => View(await _context.Recursos
                .Include(r => r.Asignatura)
                .Include(r => r.Repositorio)
                .Include(r => r.Tutor)
                .OrderByDescending(r => r.FechaSubida)
                .ToListAsync());

        public async Task<IActionResult> Formulario(int? id)
        {
            var model = id == null ? new RecursoAcademico() : await _context.Recursos.FindAsync(id);
            if (model == null) return NotFound();
            await CargarListas();
            return PartialView("_Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(RecursoAcademico model)
        {
            if (!ModelState.IsValid)
            {
                await CargarListas();
                return PartialView("_Form", model);
            }

            if (model.Id == 0) _context.Recursos.Add(model);
            else _context.Recursos.Update(model);

            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var item = await _context.Recursos.FindAsync(id);
            if (item != null)
            {
                _context.Recursos.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
