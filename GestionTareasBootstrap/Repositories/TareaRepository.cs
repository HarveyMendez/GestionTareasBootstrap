using Proyecto_1_C14644_C17853.Models;
using System.Data.SqlClient;
using System.Data;
using Proyecto_1_C14644_C17853.Utility;

namespace Proyecto_1_C14644_C17853.Repositories
{
    public class TareaRepository : ITareaRepository
    {
        // Cadena de conexión a la base de datos, desencriptada al inicializar la clase.
        private readonly string _connectionString;

        // Constructor que inicializa la cadena de conexión desencriptada.
        public TareaRepository(string connectionString)
        {
            //_connectionString = Utility.Utility.DesencriptarDato(connectionString);
            this._connectionString = connectionString;
        }

        // Método para obtener todas las tareas junto con sus usuarios asignados.
        public async Task<List<Tarea>> ObtenerTodasLasTareasAsync()
        {
            var tareas = new List<Tarea>(); // Lista para almacenar las tareas obtenidas.

            // Abre una conexión a la base de datos.
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Ejecuta el procedimiento almacenado "sp_ObtenerTareasConUsuarios".
                using (var command = new SqlCommand("sp_ObtenerTareasConUsuarios", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Lee los resultados del procedimiento almacenado.
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int idTarea = (int)reader["Id_Tarea"]; // Obtiene el ID de la tarea.

                            // Busca si la tarea ya existe en la lista para evitar duplicados.
                            var tarea = tareas.Find(t => t.Id_Tarea == idTarea);

                            // Si la tarea no existe, crea un nuevo objeto Tarea y lo agrega a la lista.
                            if (tarea == null)
                            {
                                tarea = new Tarea
                                {
                                    Id_Tarea = idTarea,
                                    Titulo = reader["Titulo"].ToString(),
                                    Descripcion = reader["Descripcion"].ToString(),
                                    FechaVencimiento = (DateTime)reader["FechaVencimiento"],
                                    Prioridad = reader["Prioridad"].ToString(),
                                    Estado = reader["Estado"].ToString(),
                                    UsuariosAsignados = new List<Usuario>() // Inicializa la lista de usuarios asignados.
                                };
                                tareas.Add(tarea);
                            }

                            // Si hay un usuario asignado a la tarea, crea un objeto Usuario y lo agrega a la lista de usuarios asignados.
                            if (reader["Id_Usuario"] != DBNull.Value)
                            {
                                var usuario = new Usuario
                                {
                                    Id_Usuario = (int)reader["Id_Usuario"],
                                    Nombre = reader["Nombre_Usuario"].ToString(),
                                    Email = reader["Email_Usuario"].ToString()
                                };
                                tarea.UsuariosAsignados.Add(usuario);
                            }
                        }
                    }
                }
            }

            return tareas; // Retorna la lista de tareas con sus usuarios asignados.
        }

        // Método para agregar una nueva tarea a la base de datos.
        public async Task<int> AgregarTareaAsync(Tarea tarea)
        {
            int idTarea = 0; // Variable para almacenar el ID de la tarea creada.

            // Abre una conexión a la base de datos.
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Ejecuta el procedimiento almacenado "sp_CrearTarea" para insertar la tarea.
                using (var command = new SqlCommand("sp_CrearTarea", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Agrega los parámetros necesarios para crear la tarea.
                    command.Parameters.AddWithValue("@Titulo", tarea.Titulo);
                    command.Parameters.AddWithValue("@Descripcion", tarea.Descripcion);
                    command.Parameters.AddWithValue("@FechaVencimiento", tarea.FechaVencimiento);
                    command.Parameters.AddWithValue("@Prioridad", tarea.Prioridad);
                    command.Parameters.AddWithValue("@Estado", tarea.Estado);

                    // Ejecuta el procedimiento y obtiene el ID de la tarea creada.
                    object result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        idTarea = Convert.ToInt32(result);
                    }
                }


                // Si la tarea fue creada exitosamente, asigna los usuarios a la tarea.
                if (idTarea > 0)
                {
                    foreach (var user in tarea.UsuariosAsignados)
                    {
                        // Ejecuta el procedimiento almacenado "sp_InsertarEliminarUsuarioTarea" para asignar usuarios.
                        using (var command = new SqlCommand("sp_InsertarEliminarUsuarioTarea", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            // Agrega los parámetros necesarios para asignar usuarios.
                            command.Parameters.AddWithValue("@Id_Tarea", idTarea);
                            command.Parameters.AddWithValue("@Id_Usuario", user.Id_Usuario);
                            command.Parameters.Add(new SqlParameter("@Operacion", SqlDbType.Bit));
                            command.Parameters["@Operacion"].Value = true;

                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }

            return idTarea; // Retorna el ID de la tarea creada.
        }
        // Método para actualizar una tarea existente, incluyendo la asignación y eliminación de usuarios.
        public async Task<bool> ActualizarTareaAsync(Tarea tarea, List<int> NoAssigned)
        {
            // Abre una conexión a la base de datos.
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Actualiza los datos principales de la tarea.
                using (var command = new SqlCommand("sp_ActualizarTarea", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Agrega los parámetros necesarios para actualizar la tarea.
                    command.Parameters.AddWithValue("@Id_Tarea", tarea.Id_Tarea);
                    command.Parameters.AddWithValue("@Titulo", tarea.Titulo);
                    command.Parameters.AddWithValue("@Descripcion", tarea.Descripcion);
                    command.Parameters.AddWithValue("@FechaVencimiento", tarea.FechaVencimiento);
                    command.Parameters.AddWithValue("@Prioridad", tarea.Prioridad);
                    command.Parameters.AddWithValue("@Estado", tarea.Estado);

                    await command.ExecuteNonQueryAsync(); // Ejecuta el procedimiento almacenado.
                }

                // Inserta los usuarios asignados a la tarea.
                using (var command = new SqlCommand("sp_InsertarEliminarUsuarioTarea", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Agrega los parámetros necesarios para insertar usuarios.
                    command.Parameters.Add(new SqlParameter("@Id_Tarea", tarea.Id_Tarea));
                    command.Parameters.Add(new SqlParameter("@Id_Usuario", SqlDbType.Int));
                    command.Parameters.Add(new SqlParameter("@Operacion", SqlDbType.Bit));

                    foreach (var user in tarea.UsuariosAsignados)
                    {
                        command.Parameters["@Id_Usuario"].Value = user.Id_Usuario;
                        command.Parameters["@Operacion"].Value = true;
                        await command.ExecuteNonQueryAsync();
                    }
                }

                // Elimina los usuarios que ya no están asignados a la tarea.
                using (var command = new SqlCommand("sp_InsertarEliminarUsuarioTarea", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Agrega los parámetros necesarios para eliminar usuarios.
                    command.Parameters.Add(new SqlParameter("@Id_Tarea", tarea.Id_Tarea));
                    command.Parameters.Add(new SqlParameter("@Id_Usuario", SqlDbType.Int));
                    command.Parameters.Add(new SqlParameter("@Operacion", SqlDbType.Bit));

                    foreach (var userId in NoAssigned)
                    {
                        command.Parameters["@Id_Usuario"].Value = userId;
                        command.Parameters["@Operacion"].Value = false; // Indica que es una operación de eliminación.
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }

            return true; // Retorna verdadero si la actualización fue exitosa.
        }

        // Método para eliminar una tarea por su ID.
        public async Task<bool> EliminarTareaAsync(int idTarea)
        {
            // Abre una conexión a la base de datos.
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Ejecuta el procedimiento almacenado "sp_EliminarTarea" para eliminar la tarea.
                using (var command = new SqlCommand("sp_EliminarTarea", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id_Tarea", idTarea);

                    int rowsAffected = await command.ExecuteNonQueryAsync(); // Ejecuta el procedimiento y obtiene el número de filas afectadas.

                    return rowsAffected > 0; // Retorna verdadero si al menos una fila fue afectada.
                }
            }
        }

        // Método para actualizar el estado de una tarea.
        public async Task<bool> ActualizarEstadoTarea(int idTarea, string estado)
        {
            try
            {
                // Abre una conexión a la base de datos.
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Ejecuta el procedimiento almacenado "sp_ActualizarEstadoTarea" para actualizar el estado.
                    using (var command = new SqlCommand("sp_ActualizarEstadoTarea", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Id_Tarea", idTarea);
                        command.Parameters.AddWithValue("@Estado", estado);

                        await command.ExecuteNonQueryAsync(); // Ejecuta el procedimiento.
                        return true; // Retorna verdadero si la actualización fue exitosa.
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores.
                Console.WriteLine($"Error al actualizar el estado de la tarea: {ex.Message}");
                return false;
            }
        }

        // Método para obtener una tarea específica por su ID.
        public async Task<Tarea> ObtenerTareaPorIdAsync(int idTarea)
        {
            Tarea tarea = null; // Variable para almacenar la tarea obtenida.

            // Abre una conexión a la base de datos.
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_ObtenerTareaId", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@pId_Tarea", idTarea);

                    await connection.OpenAsync();

                    // Lee los resultados del procedimiento almacenado.
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (tarea == null)
                            {
                                // Crea un nuevo objeto Tarea con los datos obtenidos.
                                tarea = new Tarea
                                {
                                    Id_Tarea = reader.GetInt32(reader.GetOrdinal("Id_Tarea")),
                                    Titulo = reader.GetString(reader.GetOrdinal("Titulo")),
                                    Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
                                    FechaVencimiento = reader.GetDateTime(reader.GetOrdinal("FechaVencimiento")),
                                    Prioridad = reader.GetString(reader.GetOrdinal("Prioridad")),
                                    Estado = reader.GetString(reader.GetOrdinal("Estado")),
                                    UsuariosAsignados = new List<Usuario>() // Inicializa la lista de usuarios asignados.
                                };
                            }

                            // Si hay un usuario asignado a la tarea, crea un objeto Usuario y lo agrega a la lista.
                            if (!reader.IsDBNull(reader.GetOrdinal("Id_Usuario")))
                            {
                                var usuario = new Usuario
                                {
                                    Id_Usuario = reader.GetInt32(reader.GetOrdinal("Id_Usuario")),
                                    Nombre = reader.GetString(reader.GetOrdinal("Nombre_Usuario")),
                                    Email = reader.GetString(reader.GetOrdinal("Email_Usuario"))
                                };
                                tarea.UsuariosAsignados.Add(usuario);
                            }
                        }
                    }
                }
            }

            return tarea; // Retorna la tarea obtenida.
        }
    }
}
