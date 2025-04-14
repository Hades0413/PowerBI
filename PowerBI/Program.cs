using Hangfire;
using Hangfire.Dashboard;
using Microsoft.EntityFrameworkCore;
using PowerBI.Data;
using PowerBI.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// 🔧 Configuración inicial del servidor
// =====================================================================

// Escuchar en el puerto 5000 en cualquier IP
builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(5000));

// =====================================================================
// 🔧 Registro de servicios
// =====================================================================

builder.Services.AddTransient<ExcelService>(); // Servicio para manejar archivos Excel y TXT
builder.Services.AddScoped<TareaProgramadaService>(); // Servicio de tareas automáticas

// Configuración de Entity Framework con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sql")));

// Soporte para controladores y vistas Razor
builder.Services.AddControllersWithViews();

// Configuración de Hangfire con almacenamiento en SQL Server
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("sql")));

// Configuración del servidor Hangfire
builder.Services.AddHangfireServer(options =>
{
    options.SchedulePollingInterval = TimeSpan.FromSeconds(5); // Verifica trabajos pendientes cada 5 seg
    options.ServerCheckInterval = TimeSpan.FromSeconds(5); // Verifica el estado del servidor cada 5 seg
});

// =====================================================================
// 🚀 Construcción de la aplicación
// =====================================================================

var app = builder.Build();

// =====================================================================
// 🌐 Configuración del pipeline HTTP
// =====================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Ruta por defecto del MVC
app.MapControllerRoute(
    "default",
    "{controller=RotacionObreros}/{action=Index}/{id?}");


// =====================================================================
// 📊 Configuración del Dashboard de Hangfire
// =====================================================================

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Tareas Programadas Confipetrol",
    Authorization = new[] { new HangfireAuthorizationFilter() },
    StatsPollingInterval = 5000 // 5 segundos para actualizar estadísticas
});

// =====================================================================
// ⏰ Configuración de tareas programadas
// =====================================================================

// Zona horaria de Perú
var peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

// Hora de ejecución de la tarea diaria
var horaEjecucion = new TimeSpan(3, 10, 0); // 3:10 AM hora Perú

// Registrar tarea recurrente diaria con Hangfire
RecurringJob.AddOrUpdate<TareaProgramadaService>(
    "importacion-diaria-datos",
    x => x.EjecutarTareaProgramada(),
    Cron.Daily(horaEjecucion.Hours, horaEjecucion.Minutes),
    peruTimeZone
);

// Mostrar información inicial al iniciar el sistema
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<TareaProgramadaService>();

    Console.WriteLine("\n===============================================================================");
    Console.WriteLine("     ✅ Sistema de Importación Automática de Datos");
    Console.WriteLine("===============================================================================");
    Console.WriteLine($"    🕒 Hora del servidor: {DateTime.Now:HH:mm} ({TimeZoneInfo.Local.DisplayName})");
    Console.WriteLine($"    🌍 Zona horaria configurada: {peruTimeZone.DisplayName}");
    await service.ImprimirTiempoRestante(horaEjecucion);
    Console.WriteLine("===============================================================================\n");
}

app.Run();

/// <summary>
///     Filtro de autorización para el dashboard de Hangfire.
///     Actualmente permite acceso libre (desarrollo).
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Aquí se puede implementar lógica personalizada (por ejemplo, validación por roles)
        return true;
    }
}