using System.Globalization;
using ClosedXML.Excel;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using PowerBI.Data;
using PowerBI.Services;

namespace PowerBI.Controllers;

/// <summary>
///     Controlador encargado de gestionar las vistas, importaciones y exportaciones
///     relacionadas con la rotación de obreros.
/// </summary>
public class RotacionObrerosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ExcelService _excelService;
    private readonly ILogger<RotacionObrerosController> _logger;
    private readonly ICompositeViewEngine _viewEngine;


    /// <summary>
    ///     Constructor del controlador con inyección de dependencias.
    /// </summary>
    public RotacionObrerosController(
        ICompositeViewEngine viewEngine,
        ApplicationDbContext context,
        ExcelService excelService,
        ILogger<RotacionObrerosController> logger)
    {
        _viewEngine = viewEngine;
        _context = context;
        _excelService = excelService;
        _logger = logger;
    }

    /// <summary>
    ///     Vista principal que muestra todos los registros de rotación de obreros.
    /// </summary>
    public IActionResult Index()
    {
        var data = _context.RotacionObreros.ToList();
        return View(data);
    }

    /// <summary>
    ///     Carga de un archivo Excel con registros de obreros y guardado en la base de datos.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Index(IFormFile archivoExcel)
    {
        // 1. Verificar si no se seleccionó archivo
        if (archivoExcel == null || archivoExcel.Length == 0)
        {
            TempData["Error"] = "Debe seleccionar un archivo Excel para cargar.";
            return RedirectToAction("Index");
        }

        // 2. Verificar si el archivo no es un archivo Excel válido (.xlsx)
        if (Path.GetExtension(archivoExcel.FileName).ToLower() != ".xlsx")
        {
            TempData["Error"] =
                "El archivo seleccionado no es un archivo Excel válido. Por favor, seleccione un archivo con la extensión .xlsx.";
            return RedirectToAction("Index");
        }

        // 3. Mostrar mensaje de progreso al procesar el archivo
        TempData["Info"] = "El archivo está siendo cargado. Por favor, espere.";

        try
        {
            // 4. Intentar leer el archivo Excel
            var registros = _excelService.LeerRotacionObreros(archivoExcel);

            // 5. Validar que los registros no estén vacíos
            if (registros == null || registros.Count == 0)
            {
                TempData["Error"] = "El archivo Excel no contiene datos válidos o está vacío.";
                return RedirectToAction("Index");
            }

            // 6. Guardar los registros en la base de datos
            await _context.RotacionObreros.AddRangeAsync(registros);
            await _context.SaveChangesAsync();

            // 7. Si todo sale bien, mostrar mensaje de éxito
            TempData["Success"] = "Archivo cargado correctamente.";
        }
        catch (Exception ex)
        {
            // 8. Capturar errores y proporcionar un mensaje adecuado
            TempData["Error"] = $"Hubo un error al procesar el archivo: {ex.Message}.";
        }

        // 9. Redirigir a la vista
        return RedirectToAction("Index");
    }


    /// <summary>
    ///     Exporta los registros de obreros a un archivo Excel descargable.
    /// </summary>
    public IActionResult ExportarExcel()
    {
        try
        {
            var rotacionObreros = _context.RotacionObreros.ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet("RotacionObreros");

            // Encabezados
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Unidad";
            worksheet.Cell(1, 3).Value = "Servicio";
            worksheet.Cell(1, 4).Value = "Mes";
            worksheet.Cell(1, 5).Value = "NumeroPersonal";
            worksheet.Cell(1, 6).Value = "Apellidos y Nombres";
            worksheet.Cell(1, 7).Value = "Cargo";
            worksheet.Cell(1, 8).Value = "FechaIngreso";
            worksheet.Cell(1, 9).Value = "FechaCese";
            worksheet.Cell(1, 10).Value = "RelacionLaboral";
            worksheet.Cell(1, 11).Value = "DetalleRenunciaCompleto";
            worksheet.Cell(1, 12).Value = "DetalleRenuncia";
            worksheet.Cell(1, 13).Value = "TiempoMeses";
            worksheet.Cell(1, 14).Value = "Permanencia";

            // Contenido
            for (var i = 0; i < rotacionObreros.Count; i++)
            {
                var obrero = rotacionObreros[i];
                worksheet.Cell(i + 2, 1).Value = obrero.Id;
                worksheet.Cell(i + 2, 2).Value = obrero.Unidad;
                worksheet.Cell(i + 2, 3).Value = obrero.Servicio;
                worksheet.Cell(i + 2, 4).Value = obrero.Mes;
                worksheet.Cell(i + 2, 5).Value = obrero.NumeroPersonal;
                worksheet.Cell(i + 2, 6).Value = obrero.ApellidosNombres;
                worksheet.Cell(i + 2, 7).Value = obrero.Cargo;
                worksheet.Cell(i + 2, 8).Value =
                    obrero.FechaIngreso.ToString("dd MMM yyyy", CultureInfo.CurrentCulture);
                worksheet.Cell(i + 2, 9).Value = obrero.FechaCese.ToString("dd MMM yyyy", CultureInfo.CurrentCulture);
                worksheet.Cell(i + 2, 10).Value = obrero.RelacionLaboral;
                worksheet.Cell(i + 2, 11).Value = obrero.DetalleRenunciaCompleto;
                worksheet.Cell(i + 2, 12).Value = obrero.DetalleRenuncia;
                worksheet.Cell(i + 2, 13).Value = obrero.TiempoMeses;
                worksheet.Cell(i + 2, 14).Value = obrero.Permanencia;
            }

            // Descarga del archivo
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            TempData["Success"] = "Archivo Excel generado correctamente.";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "RotacionObreros.xlsx");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al generar el archivo Excel: {ex.Message}";
            return View("Error");
        }
    }

    /// <summary>
    ///     Exporta los registros de obreros a un archivo PDF descargable.
    /// </summary>
    public async Task<IActionResult> ExportarPDF()
    {
        try
        {
            // Obtener los registros de la base de datos
            var rotacionObreros = await _context.RotacionObreros.AsNoTracking().ToListAsync();

            // Generar el contenido HTML utilizando una vista Razor como plantilla
            var htmlContent = await RenderViewToStringAsync("PDFTemplate", rotacionObreros);

            // Configuración para la conversión de HTML a PDF
            var converter = new SynchronizedConverter(new PdfTools());
            var doc = new HtmlToPdfDocument
            {
                GlobalSettings =
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                    Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 }
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = htmlContent, // El contenido HTML que se convertirá a PDF
                        WebSettings =
                        {
                            DefaultEncoding = "utf-8",
                            PrintMediaType = true
                        },
                        HeaderSettings = { FontSize = 9, Right = "Página [page] de [toPage]", Spacing = 5 }
                    }
                }
            };

            // Convertir el documento HTML a bytes en formato PDF
            var pdfBytes = converter.Convert(doc);

            // Devolver el archivo PDF generado
            return File(pdfBytes, "application/pdf",
                $"RotacionObreros_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }
        catch (Exception ex)
        {
            // Registrar el error y retornar una vista de error
            _logger.LogError(ex, "Error generando PDF");
            TempData["Error"] = "Error al generar archivo PDF";
            return RedirectToAction(nameof(Index));
        }
    }


    /// <summary>
    ///     Convierte una vista Razor en string HTML para renderizar en PDF.
    /// </summary>
    private async Task<string> RenderViewToStringAsync(string viewName, object model)
    {
        var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
        if (!viewResult.Success) throw new FileNotFoundException($"Vista '{viewName}' no encontrada.");

        var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };

        using var writer = new StringWriter();
        var viewContext = new ViewContext(
            ControllerContext,
            viewResult.View,
            viewDictionary,
            TempData,
            writer,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return writer.ToString();
    }

    /// <summary>
    ///     Importa registros de rotación de obreros desde un archivo TXT en disco.
    /// </summary>
    public async Task<IActionResult> ImportarDesdeTxt()
    {
        var rutaTxt = "/mnt/Disco2/Confipetrol/txt/datos.txt";

        try
        {
            var registros = _excelService.LeerRotacionObrerosDesdeTxt(rutaTxt);

            if (registros.Count > 0)
            {
                await _context.RotacionObreros.AddRangeAsync(registros);
                await _context.SaveChangesAsync();

                Console.WriteLine("Datos importados desde el archivo TXT correctamente.");
                TempData["Success"] = "Datos importados desde el archivo TXT correctamente.";
            }
            else
            {
                Console.WriteLine("El archivo TXT no contiene datos válidos.");
                TempData["Error"] = "El archivo TXT no contiene datos válidos.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al procesar el archivo TXT: {ex.Message}");
            TempData["Error"] = $"Error al procesar el archivo TXT: {ex.Message}";
        }

        return RedirectToAction("Index");
    }
}