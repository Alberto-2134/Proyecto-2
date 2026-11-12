using EcosistemaAcademico.Models.Data;
using EcosistemaAcademico.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcosistemaAcademico.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AsignaturasController : Controller
    {
        private readonly AppDbContext _context;
        public AsignaturasController(AppDbContext context) { _context = context; }

        // LEER: lista
        public async Task<IActionResult> Index()
            => View(await _context.Asignaturas.OrderBy(a => a.Nombre).ToListAsync());

        // Devuelve el formulario para el modal (vacío o con datos)
        public async Task<IActionResult> Formulario(int? id)
        {
            var model = id == null ? new Asignatura() : await _context.Asignaturas.FindAsync(id);
            if (model == null) return NotFound();
            return PartialView("_Form", model);
        }

        // CREAR / ACTUALIZAR
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(Asignatura model)
        {
            if (!ModelState.IsValid) return PartialView("_Form", model);

            if (model.Id == 0) _context.Asignaturas.Add(model);
            else _context.Asignaturas.Update(model);

            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        // ELIMINAR
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var item = await _context.Asignaturas.FindAsync(id);
            if (item != null)
            {
                try
                {
                    _context.Asignaturas.Remove(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "No se puede eliminar: la asignatura está en uso.";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}