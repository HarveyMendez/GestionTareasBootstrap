using Proyecto_1_C14644_C17853.Models;
using System.Globalization;
using System.Text;

namespace Proyecto_1_C14644_C17853.Utility
{
    public class Utility
    {
        // Constantes que representan los estados posibles de una tarea.
        public static string Pendiente = "pendiente";
        public static string EnProgreso = "enproceso";
        public static string Finalizado = "finalizado";

        // Método para encriptar un dato utilizando codificación Base64.
        public static string EncriptarDato(string dato)
        {
            // Convierte el dato a bytes usando UTF-8.
            byte[] bytesPlano = System.Text.Encoding.UTF8.GetBytes(dato);

            // Codifica los bytes en una cadena Base64.
            string datoEncriptado = Convert.ToBase64String(bytesPlano);

            return datoEncriptado; // Retorna el dato encriptado.
        }

        // Método estático para desencriptar un dato previamente encriptado en Base64.
        public static string DesencriptarDato(string datoEncriptado)
        {
            try
            {
                // Decodifica la cadena Base64 a bytes.
                byte[] bytesDecodificados = Convert.FromBase64String(datoEncriptado);

                // Convierte los bytes de vuelta a una cadena UTF-8.
                string datoDesencriptado = System.Text.Encoding.UTF8.GetString(bytesDecodificados);

                return datoDesencriptado; // Retorna el dato desencriptado.
            }
            catch (FormatException e)
            {
                // Maneja errores si la cadena no es válida para decodificar.
                Console.WriteLine("Error al desencriptar la cadena de conexión: " + e.Message);
                throw; // Lanza la excepción para que sea manejada por el llamador.
            }
        }

        // Método estático para generar una contraseña aleatoria de 8 caracteres.
        public static string GenerarContraseña()
        {
            const string caracteres = "abcdefghijklmnopqrstuvwxyz0123456789"; // Conjunto de caracteres permitidos.
            var random = new Random(); // Generador de números aleatorios.
            var contraseña = new StringBuilder(); // Constructor de cadenas para construir la contraseña.
            const int longitud = 8; // Longitud fija de la contraseña.

            // Genera una contraseña aleatoria seleccionando caracteres del conjunto.
            for (int i = 0; i < longitud; i++)
            {
                contraseña.Append(caracteres[random.Next(caracteres.Length)]);
            }

            return contraseña.ToString(); // Retorna la contraseña generada.
        }
    }
}
