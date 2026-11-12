using EcosistemaAcademico.Models.Data;
using EcosistemaAcademico.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcosistemaAcademico.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RepositoriosController : Controller
    {
        private readonly AppDbContext _context;
        public RepositoriosController(AppDbContext context) { _context = context; }

        // Llena la lista desplegable de administradores
        private async Task CargarListas()
        {
            var admins = await _context.Administradores
                .Select(a => new { a.Id, Nombre = a.Nombre + " " + a.Apellido })
                .ToListAsync();
            ViewBag.Administradores = new SelectList(admins, "Id", "Nombre");
        }

        public async Task<IActionResult> Index()
            => View(await _context.Repositorios
                .Include(r => r.Administrador)
                .OrderBy(r => r.Nombre)
                .ToListAsync());

        public async Task<IActionResult> Formulario(int? id)
        {
            var model = id == null ? new Repositorio() : await _context.Repositorios.FindAsync(id);
            if (model == null) return NotFound();
            await CargarListas();
            return PartialView("_Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(Repositorio model)
        {
            if (!ModelState.IsValid)
            {
                await CargarListas();
                return PartialView("_Form", model);
            }

            if (model.Id == 0) _context.Repositorios.Add(model);
            else _context.Repositorios.Update(model);

            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var item = await _context.Repositorios.FindAsync(id);
            if (item != null)
            {
                try
                {
                    _context.Repositorios.Remove(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "No se puede eliminar: el repositorio tiene recursos académicos.";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
