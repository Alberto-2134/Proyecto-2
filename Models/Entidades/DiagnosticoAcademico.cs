namespace EcosistemaAcademico.Models.Entidades
{
    public class DiagnosticoAcademico
    {
        public int Id { get; set; }
        public int EstudianteId { get; set; }
        public Estudiante? Estudiante { get; set; }
        public int AsignaturaId { get; set; }
        public Asignatura? Asignatura { get; set; }
        public string NivelRiesgo { get; set; } = string.Empty; // Bajo, Medio, Alto
        public string Fortalezas { get; set; } = string.Empty;
        public string Debilidades { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Observaciones { get; set; } = string.Empty;
    }
}