namespace EcosistemaAcademico.Models.Entidades
{
    public class Administrador : Usuario
    {
        public string Departamento { get; set; } = string.Empty;
        public string NivelAcceso { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
    }
}