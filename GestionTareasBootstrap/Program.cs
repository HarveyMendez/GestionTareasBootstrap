using Proyecto_1_C14644_C17853.Repositories;
using Proyecto_1_C14644_C17853.Services;
using Proyecto_1_C14644_C17853.Utility;


var builder = WebApplication.CreateBuilder(args);
// Obtiene la cadena de conexión desde la configuración de la aplicación.
// Si no se encuentra, lanza una excepción.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string not found");

// Configura los servicios de la aplicación.
builder.Services.AddControllersWithViews(); // Agrega soporte para controladores y vistas.
builder.Services.AddSession(); // Habilita el uso de sesiones.

// Inyección de dependencias optimizada:
// Registra los repositorios y servicios como dependencias inyectables en la aplicación.
builder.Services.AddScoped<IUsuarioRepository>(sp => new UsuarioRepository(connectionString)); // Repositorio de usuarios.
builder.Services.AddScoped<ITareaRepository>(sp => new TareaRepository(connectionString)); // Repositorio de tareas.
builder.Services.AddTransient<ICorreoService, CorreoService>(); // Servicio de correo.
builder.Services.AddHttpContextAccessor(); // Acceso al contexto HTTP para manejar sesiones y solicitudes.
builder.Services.AddScoped<IUsuarioService, UsuarioService>(); // Servicio de usuario.

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=InicioSesion}/{id?}");

app.Run();