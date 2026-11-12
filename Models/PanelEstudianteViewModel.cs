using EcosistemaAcademico.Models.Entidades;

namespace EcosistemaAcademico.Models
{
    public class PanelEstudianteViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public int Nivel { get; set; }
        public decimal Promedio { get; set; }
        public int TutoriasVigentes { get; set; }
        public DateTime? ProximaSesion { get; set; }

        public List<TutoriaItem> Tutorias { get; set; } = new();
        public List<DiagnosticoAcademico> Diagnosticos { get; set; } = new();
        public List<RecursoAcademico> Recursos { get; set; } = new();
    }

    public class TutoriaItem
    {
        public string Tutor { get; set; } = string.Empty;
        public string Asignatura { get; set; } = string.Empty;
        public string Disponibilidad { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int Progreso { get; set; }
        public decimal? CalificacionInicial { get; set; }
        public decimal? CalificacionActual { get; set; }
        public DateTime? ProximaSesion { get; set; }
    }
}