namespace EcosistemaAcademico.Models.Entidades
{
    public class Estudiante : Usuario
    {
        public string Matricula { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public int Nivel { get; set; }
        public decimal Promedio { get; set; }
        public bool NecesitaIntervencion { get; set; }
    }
}