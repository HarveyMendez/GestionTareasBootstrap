using Proyecto_1_C14644_C17853.Models;

namespace Proyecto_1_C14644_C17853.Repositories
{
    public interface ITareaRepository
    {
        // Retorna una lista de objetos Tarea.
        public Task<List<Tarea>> ObtenerTodasLasTareasAsync();

        // Recibe un objeto Tarea y retorna el ID de la tarea creada como un entero.
        public Task<int> AgregarTareaAsync(Tarea tarea);

        // Recibe un objeto Tarea con los datos actualizados y una lista de IDs de usuarios que ya no están asignados.
        // Retorna un valor booleano indicando si la actualización fue exitosa.
        public Task<bool> ActualizarTareaAsync(Tarea tarea, List<int> NoAssignedTo);

        // Recibe el ID de la tarea a eliminar y retorna un valor booleano indicando si la eliminación fue exitosa.
        public Task<bool> EliminarTareaAsync(int idTarea);

        // Recibe el ID de la tarea y el nuevo estado como una cadena de texto.
        // Retorna un valor booleano indicando si la actualización del estado fue exitosa.
        public Task<bool> ActualizarEstadoTarea(int idTarea, string estado);

        // Recibe el ID de la tarea y retorna un objeto Tarea.
        public Task<Tarea> ObtenerTareaPorIdAsync(int idTarea);
    }
}
