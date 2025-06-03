using System.Net.Mail;
using System.Net;
using Proyecto_1_C14644_C17853.Models;

namespace Proyecto_1_C14644_C17853.Services
{
    public class CorreoService : ICorreoService
    {
        // Configuración de la aplicación para obtener los parámetros del correo (emisor, contraseña, host, puerto).
        private readonly IConfiguration configuration;

        // Acceso al contexto HTTP para construir URLs dinámicas.
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor que inicializa la configuración y el acceso al contexto HTTP.
        public CorreoService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // Método para enviar un correo electrónico.
        public async Task<bool> EnviarEmail(string emailReceptor, string tema, string cuerpo)
        {
            // Obtiene los valores de configuración necesarios para el correo.
            var emailEmisor = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:EMAIL");
            var password = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:PASSWORD");
            var host = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:HOST");
            var puerto = configuration.GetValue<int>("CONFIGURACIONES_EMAIL:PUERTO");

            try
            {
                // Configura el cliente SMTP para enviar el correo.
                using var smtpCliente = new SmtpClient(host, puerto)
                {
                    EnableSsl = true, // Habilita SSL para una conexión segura.
                    UseDefaultCredentials = false, // No usa las credenciales predeterminadas.
                    Credentials = new NetworkCredential(emailEmisor, password) // Usa las credenciales del emisor.
                };

                // Crea el mensaje de correo con el asunto y el cuerpo.
                var mensaje = new MailMessage(emailEmisor!, emailReceptor, tema, cuerpo)
                {
                    IsBodyHtml = true // Indica que el cuerpo del correo puede contener HTML.
                };

                // Envía el correo de forma asíncrona.
                await smtpCliente.SendMailAsync(mensaje);
                return true; // Retorna verdadero si el correo fue enviado exitosamente.
            }
            catch (Exception ex)
            {
                // Maneja errores e imprime un mensaje en la consola.
                Console.WriteLine($"Error al enviar correo a {emailReceptor}: {ex.Message}");
                return false; // Retorna falso si ocurre un error.
            }
        }

        // Método para notificar a los usuarios asignados sobre un cambio de estado en una tarea.
        public async Task<bool> NotificarCambioEstadoTarea(Tarea tarea)
        {
            // Genera una tarea de envío de correo para cada usuario asignado.
            var tareas = tarea.UsuariosAsignados.Select(usuario =>
            {
                var tema = $"Cambio de estado en la tarea: {tarea.Titulo}";
                var cuerpo = $"Hola {usuario.Nombre}, el estado de la tarea '{tarea.Titulo}' ha cambiado a {tarea.Estado}.";
                return EnviarEmail(usuario.Email, tema, cuerpo); // Envía el correo.
            });

            // Ejecuta todas las tareas de envío de correo en paralelo y espera los resultados.
            var resultados = await Task.WhenAll(tareas);

            // Retorna verdadero si todos los correos fueron enviados exitosamente.
            return resultados.All(resultado => resultado);
        }

        // Método para notificar a los usuarios sobre una nueva tarea asignada.
        public async Task<bool> NotificarTareaAsignada(List<Usuario> usuarios, Tarea tarea)
        {
            // Genera una tarea de envío de correo para cada usuario asignado.
            var tareas = usuarios.Select(usuario =>
            {
                var tema = $"Nueva tarea asignada: {tarea.Titulo}";
                var cuerpo = $"Hola {usuario.Nombre}, se te ha asignado la tarea {tarea.Titulo}.";
                return EnviarEmail(usuario.Email, tema, cuerpo); // Envía el correo.
            });

            // Ejecuta todas las tareas de envío de correo en paralelo y espera los resultados.
            var resultados = await Task.WhenAll(tareas);

            // Retorna verdadero si todos los correos fueron enviados exitosamente.
            return resultados.All(resultado => resultado);
        }

        // Método para enviar las credenciales de acceso a un usuario.
        public async Task<bool> EnviarCredencialesUsuario(Usuario usuario)
        {
            // Construye la URL base de la aplicación usando el contexto HTTP.
            var request = _httpContextAccessor.HttpContext.Request;
            var url = $"{request.Scheme}://{request.Host}{request.PathBase}";

            // Define el asunto y el cuerpo del correo con las credenciales del usuario.
            var tema = "Tus credenciales de acceso";
            var cuerpo = $"Hola {usuario.Nombre},\n\n" +
                         $"Aquí tienes tus credenciales de acceso:\n" +
                         $"Email: {usuario.Email}\n" +
                         $"Contraseña: {usuario.Password}\n\n" +
                         $"Puedes acceder a la aplicación en la siguiente dirección: {url}";

            // Envía el correo con las credenciales.
            var resultado = await EnviarEmail(usuario.Email, tema, cuerpo);

            // Retorna el resultado del envío del correo.
            return resultado;
        }
    }
}
