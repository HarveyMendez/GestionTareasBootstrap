using Microsoft.AspNetCore.Http;
using Proyecto_1_C14644_C17853.Models;
using System.Threading.Tasks;

namespace Proyecto_1_C14644_C17853.Services
{
    public class UsuarioService : IUsuarioService
    {
        // Acceso al contexto HTTP para interactuar con la sesión del usuario.
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor que inicializa el acceso al contexto HTTP.
        public UsuarioService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Método para guardar los datos del usuario actual en la sesión.
        public Task GuardarUsuarioActual(Usuario usuario)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // Almacena los datos del usuario en la sesión.
            httpContext.Session.SetInt32("Id_Usuario", usuario.Id_Usuario);
            httpContext.Session.SetString("Nombre", usuario.Nombre);
            httpContext.Session.SetString("Email", usuario.Email);
            httpContext.Session.SetString("Admin", usuario.Admin.ToString());

            // Retorna una tarea completada para indicar que la operación ha finalizado.
            return Task.CompletedTask;
        }

        // Método para obtener los datos del usuario actual almacenados en la sesión.
        public Task<Usuario> ObtenerUsuarioActual()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // Obtiene el ID del usuario y el correo electrónico almacenados en la sesión.
            var idUsuario = httpContext.Session.GetInt32("Id_Usuario");
            var email = httpContext.Session.GetString("Email");

            // Si no hay un correo electrónico en la sesión, retorna nulo.
            if (string.IsNullOrEmpty(email))
            {
                return Task.FromResult<Usuario>(null);
            }

            // Obtiene el nombre y el estado de administrador del usuario desde la sesión.
            var nombre = httpContext.Session.GetString("Nombre");
            var admin = bool.Parse(httpContext.Session.GetString("Admin"));

            // Crea un objeto Usuario con los datos obtenidos de la sesión.
            var usuario = new Usuario
            {
                Id_Usuario = idUsuario ?? 0,
                Email = email,
                Nombre = nombre,
                Admin = admin
            };

            // Retorna el usuario como resultado de la tarea.
            return Task.FromResult(usuario);
        }

        // Método para cerrar la sesión del usuario actual.
        public void CerrarSesion()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // Limpia toda la información almacenada en la sesión.
            httpContext.Session.Clear();
        }
    }
}