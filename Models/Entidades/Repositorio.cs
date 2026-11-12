namespace EcosistemaAcademico.Models.Entidades
{
    public class Repositorio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // Local, Nube, Departamental
        public int CapacidadGB { get; set; }
        public bool Estado { get; set; } = true;
        public int AdministradorId { get; set; }
        public Administrador? Administrador { get; set; }
    }
}
