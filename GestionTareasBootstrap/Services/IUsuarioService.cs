using Microsoft.AspNetCore.Mvc;
using Proyecto_1_C14644_C17853.Models;

namespace Proyecto_1_C14644_C17853.Services
{
    public interface IUsuarioService
    {
        public Task<Usuario> ObtenerUsuarioActual();
        public Task GuardarUsuarioActual(Usuario usuario);
        public void CerrarSesion();
    }
}
