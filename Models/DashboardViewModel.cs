using EcosistemaAcademico.Models.Entidades;

namespace EcosistemaAcademico.Models
{
    public class DashboardViewModel
    {
        // Tarjetas
        public int TotalEstudiantes { get; set; }
        public int TotalTutores { get; set; }
        public int TotalRecursos { get; set; }
        public int EstudiantesEnRiesgo { get; set; }
        public int EmparejamientosActivos { get; set; }
        public double MejoraPromedio { get; set; }

        // Gráfica de diagnósticos
        public int RiesgoBajo { get; set; }
        public int RiesgoMedio { get; set; }
        public int RiesgoAlto { get; set; }

        // Gráfica de emparejamientos
        public int EmpPendientes { get; set; }
        public int EmpActivos { get; set; }
        public int EmpFinalizados { get; set; }
        public int EmpCancelados { get; set; }

        // Listas
        public List<Estudiante> EstudiantesSinTutor { get; set; } = new();
        public List<CargaTutor> CargaTutores { get; set; } = new();
    }

    public class CargaTutor
    {
        public string Nombre { get; set; } = string.Empty;
        public int Activos { get; set; }
        public int Capacidad { get; set; }
    }
}
