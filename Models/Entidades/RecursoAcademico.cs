namespace EcosistemaAcademico.Models.Entidades
{
    public class RecursoAcademico
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // PDF, Video, Guía
        public int AsignaturaId { get; set; }
        public Asignatura? Asignatura { get; set; }
        public int RepositorioId { get; set; }
        public Repositorio? Repositorio { get; set; }
        public int TutorId { get; set; }
        public Tutor? Tutor { get; set; } // autor del recurso
        public string UrlRuta { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; } = DateTime.Now;
        public bool Estado { get; set; } = true;
    }
}
