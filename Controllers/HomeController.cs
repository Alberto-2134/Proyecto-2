using System.Diagnostics;
using System.Security.Claims;
using EcosistemaAcademico.Models;
using EcosistemaAcademico.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcosistemaAcademico.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Estudiante")) return await PanelEstudiante();

            // Solo el administrador ve las estadísticas generales
            if (!User.IsInRole("Administrador")) return View("Bienvenida");

            var vm = new DashboardViewModel
            {
                TotalEstudiantes = await _context.Estudiantes.CountAsync(e => e.Estado),
                TotalTutores = await _context.Tutores.CountAsync(t => t.Estado),
                TotalRecursos = await _context.Recursos.CountAsync(r => r.Estado),
                EstudiantesEnRiesgo = await _context.Estudiantes.CountAsync(e => e.NecesitaIntervencion),
                EmparejamientosActivos = await _context.Emparejamientos.CountAsync(e => e.Estado == "Activo"),

                RiesgoBajo = await _context.Diagnosticos.CountAsync(d => d.NivelRiesgo == "Bajo"),
                RiesgoMedio = await _context.Diagnosticos.CountAsync(d => d.NivelRiesgo == "Medio"),
                RiesgoAlto = await _context.Diagnosticos.CountAsync(d => d.NivelRiesgo == "Alto"),

                EmpPendientes = await _context.Emparejamientos.CountAsync(e => e.Estado == "Pendiente"),
                EmpActivos = await _context.Emparejamientos.CountAsync(e => e.Estado == "Activo"),
                EmpFinalizados = await _context.Emparejamientos.CountAsync(e => e.Estado == "Finalizado"),
                EmpCancelados = await _context.Emparejamientos.CountAsync(e => e.Estado == "Cancelado")
            };

            // Promedio de mejora (después - antes); 0 si aún no hay seguimientos
            vm.MejoraPromedio = await _context.Seguimientos
                .AverageAsync(s => (double?)(s.CalificacionDespues - s.CalificacionAntes)) ?? 0;

            // Estudiantes en riesgo que todavía no tienen un tutor vigente
            vm.EstudiantesSinTutor = await _context.Estudiantes
                .Where(e => e.NecesitaIntervencion &&
                            !_context.Emparejamientos.Any(m => m.EstudianteId == e.Id &&
                                (m.Estado == "Pendiente" || m.Estado == "Activo")))
                .OrderBy(e => e.Apellido)
                .Take(5)
                .ToListAsync();

            // Los 5 tutores con más carga
            vm.CargaTutores = await _context.Tutores
                .Where(t => t.Estado)
                .Select(t => new CargaTutor
                {
                    Nombre = t.Nombre + " " + t.Apellido,
                    Capacidad = t.CapacidadMaxima,
                    Activos = _context.Emparejamientos.Count(m => m.TutorId == t.Id &&
                        (m.Estado == "Pendiente" || m.Estado == "Activo"))
                })
                .OrderByDescending(c => c.Activos)
                .Take(5)
                .ToListAsync();

            return View(vm);
        }

        private async Task<IActionResult> PanelEstudiante()
        {
            // El Id sale de la sesión: el estudiante solo puede ver lo suyo
            var id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante == null) return View("Bienvenida");

            var emparejamientos = await _context.Emparejamientos
                .Include(e => e.Tutor)
                .Include(e => e.Asignatura)
                .Where(e => e.EstudianteId == id && e.Estado != "Cancelado")
                .OrderByDescending(e => e.Fecha)
                .ToListAsync();

            var empIds = emparejamientos.Select(e => e.Id).ToList();
            var seguimientos = await _context.Seguimientos
                .Where(s => empIds.Contains(s.EmparejamientoId))
                .ToListAsync();

            var tutorias = emparejamientos.Select(e =>
            {
                var segs = seguimientos
                    .Where(s => s.EmparejamientoId == e.Id)
                    .OrderBy(s => s.Fecha)
                    .ToList();

                return new TutoriaItem
                {
                    Tutor = $"{e.Tutor?.Nombre} {e.Tutor?.Apellido}",
                    Asignatura = e.Asignatura?.Nombre ?? "",
                    Disponibilidad = e.Tutor?.Disponibilidad ?? "",
                    Estado = e.Estado,
                    Progreso = segs.Count > 0 ? segs.Max(s => s.Progreso) : 0,
                    CalificacionInicial = segs.Count > 0 ? segs.First().CalificacionAntes : null,
                    CalificacionActual = segs.Count > 0 ? segs.Last().CalificacionDespues : null,
                    ProximaSesion = segs.LastOrDefault()?.ProximaSesion
                };
            }).ToList();

            var diagnosticos = await _context.Diagnosticos
                .Include(d => d.Asignatura)
                .Where(d => d.EstudianteId == id)
                .OrderByDescending(d => d.Fecha)
                .ToListAsync();

            // Recursos de las asignaturas donde tiene diagnóstico o tutor
            var asignaturaIds = diagnosticos.Select(d => d.AsignaturaId)
                .Concat(emparejamientos.Select(e => e.AsignaturaId))
                .Distinct()
                .ToList();

            var recursos = await _context.Recursos
                .Include(r => r.Asignatura)
                .Include(r => r.Repositorio)
                .Where(r => r.Estado && asignaturaIds.Contains(r.AsignaturaId))
                .OrderByDescending(r => r.FechaSubida)
                .Take(8)
                .ToListAsync();

            var hoy = DateTime.Today;
            var vm = new PanelEstudianteViewModel
            {
                NombreCompleto = $"{estudiante.Nombre} {estudiante.Apellido}",
                Carrera = estudiante.Carrera,
                Nivel = estudiante.Nivel,
                Promedio = estudiante.Promedio,
                Tutorias = tutorias,
                TutoriasVigentes = tutorias.Count(t => t.Estado == "Pendiente" || t.Estado == "Activo"),
                ProximaSesion = tutorias
                    .Where(t => t.Estado != "Finalizado" && t.ProximaSesion >= hoy)
                    .OrderBy(t => t.ProximaSesion)
                    .Select(t => t.ProximaSesion)
                    .FirstOrDefault(),
                Diagnosticos = diagnosticos,
                Recursos = recursos
            };

            return View("PanelEstudiante", vm);
        }

        public IActionResult Privacy() => View();

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}