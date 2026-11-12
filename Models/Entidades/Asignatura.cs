namespace EcosistemaAcademico.Models.Entidades
{
    public class Asignatura
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Creditos { get; set; }
        public string Area { get; set; } = string.Empty;
        public int Nivel { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool Estado { get; set; } = true;
    }
}
