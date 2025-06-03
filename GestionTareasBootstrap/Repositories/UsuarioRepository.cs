using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Proyecto_1_C14644_C17853.Models;
using Proyecto_1_C14644_C17853.Utility;

namespace Proyecto_1_C14644_C17853.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        // Cadena de conexión a la base de datos, desencriptada al inicializar la clase.
        private readonly string _connectionString;

        // Constructor que inicializa la cadena de conexión desencriptada.
        public UsuarioRepository(string connectionString)
        {
            //_connectionString = Utility.Utility.DesencriptarDato(connectionString);
            this._connectionString  = connectionString;
        }

        // Método para crear un nuevo usuario en la base de datos.
        public async Task<bool> CrearUsuario(Usuario usuario)
        {
            // Abre una conexión a la base de datos.
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Ejecuta el procedimiento almacenado "sp_CrearUsuario" para insertar el usuario.
                using (SqlCommand cmd = new SqlCommand("sp_CrearUsuario", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Agrega los parámetros necesarios para crear el usuario.
                    cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@Email", usuario.Email);
                    cmd.Parameters.AddWithValue("@Password", usuario.Password);
                    cmd.Parameters.AddWithValue("@Admin", usuario.Admin);

                    await conn.OpenAsync(); // Abre la conexión.
                    await cmd.ExecuteNonQueryAsync(); // Ejecuta el procedimiento almacenado.
                    return true; // Retorna verdadero si la creación fue exitosa.
                }
            }
        }

        // Método para actualizar los datos de un usuario existente en la base de datos.
        public async Task<bool> ActualizarUsuario(Usuario usuario)
        {
            // Abre una conexión a la base de datos.
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Ejecuta el procedimiento almacenado "sp_ActualizarUsuario" para actualizar el usuario.
                using (SqlCommand cmd = new SqlCommand("sp_ActualizarUsuario", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Agrega los parámetros necesarios para actualizar el usuario.
                    cmd.Parameters.AddWithValue("@Id_Usuario", usuario.Id_Usuario);
                    cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@Email", usuario.Email);

                    await conn.OpenAsync(); // Abre la conexión.
                    await cmd.ExecuteNonQueryAsync(); // Ejecuta el procedimiento almacenado.
                    return true; // Retorna verdadero si la actualización fue exitosa.
                }
            }
        }

        // Método para obtener un usuario por su correo electrónico y contraseña.
        public async Task<Usuario> ObtenerUsuario(string email, string password)
        {
            try
            {
                // Abre una conexión a la base de datos.
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // Ejecuta el procedimiento almacenado "sp_VerificarInicioSesion" para verificar las credenciales.
                    using (SqlCommand cmd = new SqlCommand("sp_VerificarInicioSesion", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Password", password);

                        await conn.OpenAsync(); // Abre la conexión.

                        // Lee los resultados del procedimiento almacenado.
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Si se encuentra un usuario, crea un objeto Usuario con los datos obtenidos.
                                return new Usuario
                                {
                                    Id_Usuario = reader.GetInt32(0),
                                    Nombre = reader.GetString(1),
                                    Email = reader.GetString(2),
                                    Admin = reader.GetBoolean(3)
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Maneja errores específicos de SQL, como credenciales incorrectas.
                if (ex.Message.Contains("Credenciales incorrectas"))
                {
                    Console.WriteLine("Error: Credenciales incorrectas.");
                    return null;
                }
                else
                {
                    // Registra y lanza otras excepciones de SQL.
                    Console.WriteLine($"SqlException: {ex.Message}");
                    throw;
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                // Maneja errores de índice fuera de rango.
                Console.WriteLine($"IndexOutOfRangeException: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                // Registra y lanza otras excepciones generales.
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }

            return null; // Retorna nulo si no se encuentra ningún usuario.
        }

        // Método para obtener todos los usuarios registrados en la base de datos.
        public async Task<List<Usuario>> ObtenerUsuarios()
        {
            var usuarios = new List<Usuario>(); // Lista para almacenar los usuarios obtenidos.

            // Abre una conexión a la base de datos.
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Ejecuta el procedimiento almacenado "sp_ObtenerUsuarios" para obtener todos los usuarios.
                using (SqlCommand cmd = new SqlCommand("sp_ObtenerUsuarios", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    await conn.OpenAsync(); // Abre la conexión.

                    // Lee los resultados del procedimiento almacenado.
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Crea un objeto Usuario con los datos obtenidos y lo agrega a la lista.
                            usuarios.Add(new Usuario
                            {
                                Id_Usuario = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Email = reader.GetString(2)
                            });
                        }
                    }
                }
            }

            return usuarios; // Retorna la lista de usuarios.
        }

        // Método para obtener usuarios filtrados por una lista de IDs.
        public async Task<List<Usuario>> ObtenerUsuariosPorIdsAsync(List<int> ids)
        {
            // Obtiene todos los usuarios registrados.
            var todosLosUsuarios = await ObtenerUsuarios();

            // Filtra los usuarios cuyos IDs coinciden con los proporcionados.
            var usuariosFiltrados = todosLosUsuarios.Where(u => ids.Contains(u.Id_Usuario)).ToList();

            return usuariosFiltrados; // Retorna la lista de usuarios filtrados.
        }
    }
}