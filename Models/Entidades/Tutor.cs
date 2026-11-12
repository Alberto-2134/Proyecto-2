namespace EcosistemaAcademico.Models.Entidades
{
    public class Tutor : Usuario
    {
        public string Especialidad { get; set; } = string.Empty;
        public string Disponibilidad { get; set; } = string.Empty;
        public decimal CalificacionPromedio { get; set; }
        public int CapacidadMaxima { get; set; }
    }
}
