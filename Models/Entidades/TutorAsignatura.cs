namespace EcosistemaAcademico.Models.Entidades
{
    public class TutorAsignatura
    {
        public int Id { get; set; }
        public int TutorId { get; set; }
        public Tutor? Tutor { get; set; }
        public int AsignaturaId { get; set; }
        public Asignatura? Asignatura { get; set; }
        public string NivelDominio { get; set; } = string.Empty;
        public int AniosExperiencia { get; set; }
        public DateTime FechaAsignacion { get; set; } = DateTime.Now;
        public bool Estado { get; set; } = true;
    }
}
