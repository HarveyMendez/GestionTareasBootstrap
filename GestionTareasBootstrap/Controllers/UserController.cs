using Microsoft.AspNetCore.Mvc;
using Proyecto_1_C14644_C17853.Models;
using Proyecto_1_C14644_C17853.Repositories;
using Proyecto_1_C14644_C17853.Services;

namespace Proyecto_1_C14644_C17853.Controllers
{
    public class UserController : Controller
    {
        // Inyección de dependencias para acceder a los repositorios y servicios necesarios.
        private readonly IUsuarioRepository _usuarioRepositorio;
        private readonly IUsuarioService _usuarioService;
        private readonly ICorreoService _correoService;

        // Constructor del controlador que inicializa las dependencias inyectadas.
        public UserController(IUsuarioRepository usuarioRepositorio, IUsuarioService usuarioService, ICorreoService correoService)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _usuarioService = usuarioService;
            _correoService = correoService;
        }

        // Metodo que muestra la vista para crear un nuevo usuario.
        public IActionResult CrearUsuario()
        {
            return View(); // Retorna la vista del formulario de crear usuario.
        }

        // Metodo que crea un nuevo usuario.
        [HttpPost]
        public async Task<IActionResult> CrearUsuario(Usuario usuario)
        {
            // Verifica si el modelo recibido es válido.
            if (!ModelState.IsValid)
                return BadRequest("Datos inválidos");

            try
            {
                // Genera una contraseña aleatoria y la encripta antes de guardarla.
                usuario.Password = Utility.Utility.EncriptarDato(Utility.Utility.GenerarContraseña());

                // Asegura que el usuario no tenga permisos de administrador.
                usuario.Admin = false;

                // Intenta crear el usuario en el repositorio.
                var result = await _usuarioRepositorio.CrearUsuario(usuario);

                if (result)
                {
                    // Desencripta la contraseña para enviarla por correo.
                    var usuarioCreado = usuario;
                    usuarioCreado.Password = Utility.Utility.DesencriptarDato(usuario.Password);

                    // Envía las credenciales del usuario por correo.
                    await _correoService.EnviarCredencialesUsuario(usuarioCreado);
                }

                // Redirige al mismo metodo para permitir la creación de más usuarios.
                return RedirectToAction("CrearUsuario");
            }
            catch (Exception ex)
            {
                // Manejo de errores.
                return StatusCode(500, $"Error al crear usuario: {ex.Message}");
            }
        }

        // Metodo que obtiene todos los usuarios registrados.
        [HttpGet]
        public async Task<IActionResult> ObtenerUsuarios()
        {
            try
            {
                // Obtiene la lista de usuarios desde el repositorio.
                var usuarios = await _usuarioRepositorio.ObtenerUsuarios();
                return Ok(usuarios); // Retorna la lista de usuarios.
            }
            catch (Exception ex)
            {
                // Manejo de errores.
                return StatusCode(500, $"Error al obtener los usuarios: {ex.Message}");
            }
        }

        // Metodo que muestra la pantalla de perfil del usuario actual.
        public async Task<IActionResult> PantallaPerfil()
        {
            // Obtiene al usuario actual desde el service.
            Usuario usuarioSession = await _usuarioService.ObtenerUsuarioActual();

            // Si no hay un usuario en sesión, redirige al inicio de sesión.
            if (usuarioSession == null)
            {
                return RedirectToAction("InicioSesion", "Login");
            }

            return View(usuarioSession); // Retorna la vista del perfil con los datos del usuario.
        }

        // Metodo que actualiza los datos de un usuario.
        [HttpPost]
        public async Task<IActionResult> ActualizarUsuario(Usuario usuario)
        {
            // Verifica si el modelo recibido es válido.
            if (!ModelState.IsValid)
                return BadRequest("Datos inválidos");

            try
            {
                // Actualizar el usuario en el repositorio.
                var result = await _usuarioRepositorio.ActualizarUsuario(usuario);

                if (!result)
                {
                    return BadRequest("No se pudo actualizar el usuario"); // Retorna un error si la actualización fallo.
                }

                // Guarda al usuario actualizado en el service para mantener su estado durante la sesión.
                await _usuarioService.GuardarUsuarioActual(usuario);

                // Redirige a la pantalla de perfil después de actualizar.
                return RedirectToAction("PantallaPerfil");
            }
            catch (Exception ex)
            {
                // Manejo de errores.
                return StatusCode(500, $"Error al editar usuario: {ex.Message}");
            }
        }
    }
}
