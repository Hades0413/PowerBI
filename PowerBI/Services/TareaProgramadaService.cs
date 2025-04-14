using PowerBI.Data;

namespace PowerBI.Services;

/// <summary>
///     Servicio encargado de ejecutar tareas programadas como la importación de datos
///     desde un archivo de texto hacia la base de datos.
/// </summary>
public class TareaProgramadaService
{
    private readonly ExcelService _excelService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    ///     Constructor que recibe las dependencias requeridas.
    /// </summary>
    /// <param name="excelService">Servicio que gestiona la lectura de archivos de datos.</param>
    /// <param name="serviceScopeFactory">Fábrica de scopes para el manejo de servicios con ciclo de vida corto.</param>
    public TareaProgramadaService(ExcelService excelService, IServiceScopeFactory serviceScopeFactory)
    {
        _excelService = excelService;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    ///     Calcula y muestra en consola cuánto tiempo falta para la próxima ejecución
    ///     programada de la tarea, basado en una hora específica del día.
    /// </summary>
    /// <param name="horaEjecucion">Hora del día en que se debe ejecutar la tarea.</param>
    /// <returns>Una tarea completada de forma inmediata.</returns>
    public Task ImprimirTiempoRestante(TimeSpan horaEjecucion)
    {
        var now = DateTime.Now;
        var nextRunTime = DateTime.Today.Add(horaEjecucion);

        // Si ya pasó la hora de ejecución de hoy, se programa para mañana
        if (now > nextRunTime)
            nextRunTime = nextRunTime.AddDays(1);

        var delay = nextRunTime - now;

        Console.WriteLine($"⏱ Próxima ejecución programada: {nextRunTime:HH:mm} (en {delay.TotalHours:0.00} horas)");

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Ejecuta la tarea programada: lee datos desde un archivo de texto,
    ///     los transforma en objetos y los guarda en la base de datos.
    /// </summary>
    /// <returns>Una tarea asincrónica que representa la operación de importación.</returns>
    public async Task EjecutarTareaProgramada()
    {
        try
        {
            Console.WriteLine("🚀 Iniciando ejecución programada...");

            // Crear un scope para acceder al contexto de base de datos
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var rutaTxt = "/mnt/Disco2/Confipetrol/txt/datos.txt";

                if (!File.Exists(rutaTxt))
                {
                    Console.WriteLine($"❌ Error: El archivo no existe en la ruta especificada: {rutaTxt}");
                    return;
                }

                var registros = _excelService.LeerRotacionObrerosDesdeTxt(rutaTxt);

                if (registros == null || !registros.Any())
                {
                    Console.WriteLine("⚠️ Error: El archivo está vacío o no contiene datos válidos.");
                    return;
                }

                // Inserción en base de datos
                await _context.RotacionObreros.AddRangeAsync(registros);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Se insertaron {registros.Count} registros exitosamente en la base de datos.");
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"❌ Error al acceder al archivo: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 Error crítico al ejecutar la tarea programada: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"🔍 Detalles adicionales: {ex.InnerException.Message}");
        }
    }
}