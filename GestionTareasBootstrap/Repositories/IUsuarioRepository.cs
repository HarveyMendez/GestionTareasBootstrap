using Proyecto_1_C14644_C17853.Models;

namespace Proyecto_1_C14644_C17853.Repositories
{
    public interface IUsuarioRepository
    {
        public Task<Usuario> ObtenerUsuario(string email, string password);
        public Task<bool> CrearUsuario(Usuario usuario);
        public Task<bool> ActualizarUsuario(Usuario usuario);
        public Task<List<Usuario>> ObtenerUsuarios();
        public Task<List<Usuario>> ObtenerUsuariosPorIdsAsync(List<int> ids);

    }
}
