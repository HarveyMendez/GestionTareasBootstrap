using System.ComponentModel.DataAnnotations;

namespace Proyecto_1_C14644_C17853.Models
{
    public class Usuario
    {
        public int Id_Usuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no es válido.")]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public bool Admin { get; set; } = false;
    }
}
