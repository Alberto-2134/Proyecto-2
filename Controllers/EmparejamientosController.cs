using EcosistemaAcademico.Models.Data;
using EcosistemaAcademico.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EcosistemaAcademico.Models;

namespace EcosistemaAcademico.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class EmparejamientosController : Controller
    {
        private readonly AppDbContext _context;
        public EmparejamientosController(AppDbContext context) { _context = context; }

        private async Task CargarListas()
        {
            var estudiantes = await _context.Estudiantes
                .Select(e => new { e.Id, Nombre = e.Matricula + " - " + e.Nombre + " " + e.Apellido })
                .ToListAsync();
            var tutores = await _context.Tutores
                .Select(t => new { t.Id, Nombre = t.Nombre + " " + t.Apellido })
                .ToListAsync();
            var asignaturas = await _context.Asignaturas
                .Select(a => new { a.Id, Nombre = a.Codigo + " - " + a.Nombre })
                .ToListAsync();

            ViewBag.Estudiantes = new SelectList(estudiantes, "Id", "Nombre");
            ViewBag.Tutores = new SelectList(tutores, "Id", "Nombre");
            ViewBag.Asignaturas = new SelectList(asignaturas, "Id", "Nombre");
        }

        public async Task<IActionResult> Index()
        {
            await CargarListas();
            return View(await _context.Emparejamientos
                .Include(e => e.Estudiante)
                .Include(e => e.Tutor)
                .Include(e => e.Asignatura)
                .OrderByDescending(e => e.Fecha)
                .ToListAsync());
        }

        public async Task<IActionResult> Formulario(int? id)
        {
            var model = id == null ? new Emparejamiento() : await _context.Emparejamientos.FindAsync(id);
            if (model == null) return NotFound();
            await CargarListas();
            return PartialView("_Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(Emparejamiento model)
        {
            // Regla del grafo: solo existe la arista si el tutor imparte esa asignatura
            bool imparte = await _context.TutorAsignaturas.AnyAsync(x =>
                x.TutorId == model.TutorId && x.AsignaturaId == model.AsignaturaId && x.Estado);
            if (!imparte)
                ModelState.AddModelError("", "Ese tutor no tiene asignada esa asignatura (revisa Tutor-Asignatura)");

            // No repetir un emparejamiento vigente
            bool duplicado = await _context.Emparejamientos.AnyAsync(x =>
                x.EstudianteId == model.EstudianteId &&
                x.TutorId == model.TutorId &&
                x.AsignaturaId == model.AsignaturaId &&
                (x.Estado == "Pendiente" || x.Estado == "Activo") &&
                x.Id != model.Id);
            if (duplicado)
                ModelState.AddModelError("", "Ya existe un emparejamiento vigente con esos datos");

            if (!ModelState.IsValid)
            {
                await CargarListas();
                return PartialView("_Form", model);
            }

            if (model.Id == 0) _context.Emparejamientos.Add(model);
            else _context.Emparejamientos.Update(model);

            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var item = await _context.Emparejamientos.FindAsync(id);
            if (item != null)
            {
                try
                {
                    _context.Emparejamientos.Remove(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "No se puede eliminar: tiene seguimientos académicos registrados.";
                }
            }
            return RedirectToAction(nameof(Index));
        }
        // El algoritmo: puntúa a cada tutor conectado a la asignatura
        private async Task<List<CandidatoTutor>> CalcularCandidatos(int estudianteId, int asignaturaId)
        {
            var relaciones = await _context.TutorAsignaturas
                .Include(x => x.Tutor)
                .Where(x => x.AsignaturaId == asignaturaId && x.Estado && x.Tutor!.Estado)
                .ToListAsync();

            // Cuántos emparejamientos vigentes tiene cada tutor
            var cargas = await _context.Emparejamientos
                .Where(e => e.Estado == "Pendiente" || e.Estado == "Activo")
                .GroupBy(e => e.TutorId)
                .Select(g => new { TutorId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.TutorId, x => x.Total);

            // Tutores que ya atienden a este estudiante en esta asignatura
            var yaEmparejados = await _context.Emparejamientos
                .Where(e => e.EstudianteId == estudianteId && e.AsignaturaId == asignaturaId
                            && (e.Estado == "Pendiente" || e.Estado == "Activo"))
                .Select(e => e.TutorId)
                .ToListAsync();

            var lista = new List<CandidatoTutor>();
            foreach (var r in relaciones)
            {
                var t = r.Tutor!;
                int activos = cargas.GetValueOrDefault(t.Id, 0);

                if (activos >= t.CapacidadMaxima) continue;   // sin cupo
                if (yaEmparejados.Contains(t.Id)) continue;   // ya está con este estudiante

                double dominio = r.NivelDominio switch
                {
                    "Avanzado" => 30,
                    "Intermedio" => 20,
                    _ => 10
                };
                double calificacion = (double)Math.Clamp(t.CalificacionPromedio, 0m, 5m) / 5 * 30;
                double experiencia = Math.Min(r.AniosExperiencia, 10) / 10.0 * 20;
                double carga = (1.0 - (double)activos / t.CapacidadMaxima) * 20;

                lista.Add(new CandidatoTutor
                {
                    TutorId = t.Id,
                    Nombre = t.Nombre + " " + t.Apellido,
                    Disponibilidad = t.Disponibilidad,
                    NivelDominio = r.NivelDominio,
                    AniosExperiencia = r.AniosExperiencia,
                    Calificacion = t.CalificacionPromedio,
                    CuposLibres = t.CapacidadMaxima - activos,
                    Puntaje = Math.Round(dominio + calificacion + experiencia + carga, 1)
                });
            }

            return lista.OrderByDescending(c => c.Puntaje).ToList();
        }

        // Muestra el ranking de tutores
        public async Task<IActionResult> Sugerencias(int estudianteId, int asignaturaId)
        {
            var candidatos = await CalcularCandidatos(estudianteId, asignaturaId);
            return PartialView("_Sugerencias", candidatos);
        }

        // Crea el emparejamiento con el tutor elegido
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Asignar(int estudianteId, int tutorId, int asignaturaId)
        {
            var candidato = (await CalcularCandidatos(estudianteId, asignaturaId))
                .FirstOrDefault(c => c.TutorId == tutorId);
            if (candidato == null) return BadRequest();

            _context.Emparejamientos.Add(new Emparejamiento
            {
                EstudianteId = estudianteId,
                TutorId = tutorId,
                AsignaturaId = asignaturaId,
                PuntajeAfinidad = candidato.Puntaje,
                Fecha = DateTime.Now,
                Estado = "Pendiente"
            });
            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

    }
}
