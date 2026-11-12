namespace EcosistemaAcademico.Models.Entidades
{
    public class SeguimientoAcademico
    {
        public int Id { get; set; }
        public int EmparejamientoId { get; set; }
        public Emparejamiento? Emparejamiento { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public int Progreso { get; set; } // porcentaje 0 a 100
        public decimal CalificacionAntes { get; set; }
        public decimal CalificacionDespues { get; set; }
        public string Observaciones { get; set; } = string.Empty;
        public DateTime? ProximaSesion { get; set; }
    }
}
