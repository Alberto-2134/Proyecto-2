using System.ComponentModel.DataAnnotations;

namespace EcosistemaAcademico.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Rol { get; set; } = "Estudiante";

        // Solo estudiantes
        public string? Matricula { get; set; }
        public string? Carrera { get; set; }
        public int Nivel { get; set; } = 1;

        // Solo tutores
        public string? Especialidad { get; set; }
        public string? Disponibilidad { get; set; }
    }
}
