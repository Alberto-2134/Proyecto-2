namespace EcosistemaAcademico.Models
{
    public class CandidatoTutor
    {
        public int TutorId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Disponibilidad { get; set; } = string.Empty;
        public string NivelDominio { get; set; } = string.Empty;
        public int AniosExperiencia { get; set; }
        public decimal Calificacion { get; set; }
        public int CuposLibres { get; set; }
        public double Puntaje { get; set; }
    }
}
