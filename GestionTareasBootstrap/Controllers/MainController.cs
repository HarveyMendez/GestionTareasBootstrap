using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Proyecto_1_C14644_C17853.Models;
using Proyecto_1_C14644_C17853.Repositories;
using Proyecto_1_C14644_C17853.Services;

namespace Proyecto_1_C14644_C17853.Controllers
{
    public class MainController : Controller
    {
        // Inyección de dependencias para acceder a los repository y service.
        private readonly ITareaRepository _tareaRepositorio;
        private readonly IUsuarioRepository _usuarioRepositorio;
        private readonly ICorreoService _correoService;
        private readonly IUsuarioService _usuarioService;

        // Constructor del controlador que inicializa las dependencias inyectadas.
        public MainController(ITareaRepository tareaRepositorio, IUsuarioRepository usuarioRepository, ICorreoService correoService, IUsuarioService usuarioService)
        {
            _tareaRepositorio = tareaRepositorio;
            _usuarioRepositorio = usuarioRepository;
            _correoService = correoService;
            _usuarioService = usuarioService;
        }

        // Metodo que muestra la página principal con el tablero de tareas.
        [HttpGet]
        public async Task<IActionResult> PaginaPrincipal()
        {
            try
            {
                // Obtiene al usuario actual desde el service.
                var usuarioActual = _usuarioService.ObtenerUsuarioActual();
                ViewBag.UsuarioActual = usuarioActual;

                // Obtiene todas las tareas disponibles desde el repositorio.
                var tareas = await _tareaRepositorio.ObtenerTodasLasTareasAsync();

                // Crea un objeto Tablero con las tareas obtenidas.
                var tablero = new Tablero { Tareas = tareas };

                // Crea un ViewModel para pasar datos a la vista.
                var tableroViewModel = new TableroViewModel { Tablero = tablero, Tarea = new Tarea() };

                return View(tableroViewModel); // Retorna la vista con el tablero y las tareas.
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las tareas: {ex.Message}");
            }
        }

        // Metodo para la creación de una nueva tarea.
        [HttpPost]
        public async Task<IActionResult> CrearTarea(TableroViewModel model, List<int> AssignedTo)
        {
            // Verifica si el modelo recibido es válido.
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest($"Datos inválidos: {string.Join(", ", errors)}");
            }

            try
            {
                // Obtiene los usuarios asignados a la tarea por sus IDs.
                var usuariosAsignados = await _usuarioRepositorio.ObtenerUsuariosPorIdsAsync(AssignedTo);

                // Asigna los usuarios obtenidos a la tarea.
                model.Tarea.UsuariosAsignados = usuariosAsignados;

                // Agrega la tarea al repositorio y obtiene su ID.
                int idTarea = await _tareaRepositorio.AgregarTareaAsync(model.Tarea);

                if (idTarea > 0)
                {
                    // Notifica a los usuarios asignados sobre la nueva tarea.
                    var result = await _correoService.NotificarTareaAsignada(usuariosAsignados, model.Tarea);
                    return RedirectToAction("PaginaPrincipal"); // Redirige a la página principal después de crear la tarea.
                }

                return StatusCode(500, "No se pudo agregar la tarea"); // Retorna un error si no se pudo agregar la tarea.
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al crear la tarea: {ex.Message}"); // Manejo de errores.
            }
        }
        // Metodo para actualizar la tarea.
        [HttpPost]
        public async Task<IActionResult> ActualizarTarea(Tarea tarea, [FromForm] List<int> AssignedTo, [FromForm] List<int> NoAssignedTo)
        {
            // Verifica si el modelo recibido es válido.
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }

            try
            {
                // Obtiene los usuarios asignados a la tarea por sus IDs.
                var usuariosAsignados = await _usuarioRepositorio.ObtenerUsuariosPorIdsAsync(AssignedTo);

                // Asigna los usuarios a la tarea.
                tarea.UsuariosAsignados = usuariosAsignados;

                // Actualiza la tarea en el repositorio.
                await _tareaRepositorio.ActualizarTareaAsync(tarea, NoAssignedTo);

                return Ok(new { message = "Tarea actualizada correctamente" }); // Retorna un mensaje de éxito.
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new[] { $"Error al actualizar la tarea: {ex.Message}" } }); // Manejo de errores.
            }
        }

        // Acción DELETE que maneja la eliminación de una tarea.
        [HttpDelete]
        public async Task<IActionResult> EliminarTarea(int id)
        {

            try
            {
                // Intenta eliminar la tarea del repositorio.
                bool eliminado = await _tareaRepositorio.EliminarTareaAsync(id);

                if (eliminado)
                    return Ok(new { redirectUrl = Url.Action("PaginaPrincipal") }); // Retorna la URL para redirigir tras la eliminación.

                return NotFound(new { mensaje = "La tarea no fue encontrada." }); // Retorna un error si la tarea no existe.
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al eliminar la tarea: {ex.Message}" }); // Manejo de errores.
            }
        }

        // Acción PUT que maneja la actualización del estado de una tarea.
        [HttpPut]
        public async Task<IActionResult> ActualizarEstadoTarea([FromBody] Tarea tarea)
        {
            // Verifica si los parámetros recibidos son válidos.
            if (tarea.Id_Tarea <= 0 || string.IsNullOrEmpty(tarea.Estado))
            {
                return BadRequest(new { success = false, message = "Parámetros inválidos" });
            }

            try
            {
                // Actualiza el estado de la tarea en el repositorio.
                bool actualizado = await _tareaRepositorio.ActualizarEstadoTarea(tarea.Id_Tarea, tarea.Estado);

                if (actualizado)
                {
                    // Obtiene la tarea actualizada y notifica el cambio de estado a los usuarios.
                    var tareaActualizada = await _tareaRepositorio.ObtenerTareaPorIdAsync(tarea.Id_Tarea);
                    var result = await _correoService.NotificarCambioEstadoTarea(tareaActualizada);

                    return Json(new { success = true }); // Retorna un mensaje de éxito.
                }
                else
                {
                    return NotFound(new { success = false, message = "Tarea no encontrada" }); // Retorna un error si la tarea no existe.
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno", error = ex.Message }); // Manejo de errores.
            }
        }
    }
}