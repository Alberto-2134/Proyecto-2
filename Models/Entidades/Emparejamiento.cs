namespace EcosistemaAcademico.Models.Entidades
{
    public class Emparejamiento
    {
        public int Id { get; set; }
        public int EstudianteId { get; set; }
        public Estudiante? Estudiante { get; set; }
        public int TutorId { get; set; }
        public Tutor? Tutor { get; set; }
        public int AsignaturaId { get; set; }
        public Asignatura? Asignatura { get; set; }
        public double PuntajeAfinidad { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "Pendiente";
    }
}