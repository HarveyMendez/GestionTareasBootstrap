namespace Proyecto_1_C14644_C17853.Models
{
    public class Tarea
    {
        public int Id_Tarea { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Prioridad { get; set; }
        public string Estado { get; set; }
        public List<Usuario> UsuariosAsignados { get; set; } = new List<Usuario>();
    }
}
