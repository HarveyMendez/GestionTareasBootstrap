using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Proyecto_1_C14644_C17853.Models;
using Proyecto_1_C14644_C17853.Repositories;
using Proyecto_1_C14644_C17853.Services;

namespace Proyecto_1_C14644_C17853.Controllers
{
    public class LoginController : Controller
    {
        // Inyección de dependencias para obtener los services y repositories.
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUsuarioService _usuarioService;

        // Constructor que inicializa las dependencias inyectadas.
        public LoginController(IUsuarioRepository usuarioRepository, IUsuarioService usuarioService)
        {
            _usuarioRepository = usuarioRepository;
            _usuarioService = usuarioService;
        }

        // Metodo que muestra la página de inicio de sesión.
        public IActionResult InicioSesion()
        {
            return View();
        }

        // Metodo que cierra la sesión del usuario.
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();//Limpia el usuario en sesion
            return RedirectToAction("InicioSesion");//Redirecciona a al controlador InicioSesion
        }

        // Metodo que envía los datos de inicio de sesión.
        [HttpPost]
        public async Task<IActionResult> SignIn(LoginUsuario loginUsuario)
        {
            // Verifica si el modelo recibido es válido.
            if (!ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "Por favor, completa todos los campos correctamente.";
                return View("InicioSesion", loginUsuario);// Retorna la vista de inicio de sesión con el mensaje de error.
            }

            // Busca al usuario en la base de datos.
            Usuario user = await _usuarioRepository.ObtenerUsuario(
                loginUsuario.Email,
                //Utility.Utility.EncriptarDato(loginUsuario.Password)
                loginUsuario.Password
            );

            if (user is null)
            {
                ViewData["ErrorMessage"] = "Email o contraseña incorrectos.";
                return View("InicioSesion", loginUsuario);// Retorna la vista de inicio de sesión con el mensaje de error.
            }

            // Guarda al usuario actual en el servicio para mantener su estado durante la sesión.
            await _usuarioService.GuardarUsuarioActual(user);

            // Redirige al usuario a la página principal después de iniciar sesión exitosamente.
            return RedirectToAction("PaginaPrincipal", "Main");
        }
    }
}
