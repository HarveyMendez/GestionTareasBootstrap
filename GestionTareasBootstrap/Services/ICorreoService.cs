using Proyecto_1_C14644_C17853.Models;

namespace Proyecto_1_C14644_C17853.Services
{
    public interface ICorreoService
    {
        public Task<bool> EnviarEmail(string emailReceptor, string tema, string cuerpo);
        public Task<bool> NotificarTareaAsignada(List<Usuario>usuarios, Tarea tarea);
        public Task<bool> NotificarCambioEstadoTarea(Tarea tarea);
        public Task<bool> EnviarCredencialesUsuario(Usuario usuario);

    }
}
