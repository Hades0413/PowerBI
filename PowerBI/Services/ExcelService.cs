using System.Globalization;
using ClosedXML.Excel;
using PowerBI.Models;

namespace PowerBI.Services;

/// <summary>
///     Servicio que permite la lectura y transformación de datos de rotación de obreros
///     desde archivos Excel o archivos de texto plano.
/// </summary>
public class ExcelService
{
    /// <summary>
    ///     Lee un archivo Excel que contiene información de rotación de obreros
    ///     y transforma su contenido en una lista de objetos <see cref="RotacionObrero" />.
    /// </summary>
    /// <param name="archivoExcel">Archivo Excel subido a través de un formulario.</param>
    /// <returns>Lista de objetos <see cref="RotacionObrero" /> procesados desde el archivo.</returns>
    public List<RotacionObrero> LeerRotacionObreros(IFormFile archivoExcel)
    {
        var lista = new List<RotacionObrero>();

        using (var stream = archivoExcel.OpenReadStream())
        using (var workbook = new XLWorkbook(stream))
        {
            var hoja = workbook.Worksheet(1); // Se asume que los datos están en la primera hoja
            var filas = hoja.RangeUsed().RowsUsed().Skip(1); // Se omite la fila de encabezado

            foreach (var fila in filas)
            {
                // Ignora filas vacías
                if (fila.Cell(1).IsEmpty()) continue;

                // Lectura y limpieza de campos
                var unidad = fila.Cell(1).GetString().Trim();
                var servicio = fila.Cell(2).GetString().Trim();
                var mesStr = fila.Cell(3).GetString().Trim();
                var numeroPersonal = fila.Cell(4).GetString().Trim();
                var apellidosNombres = fila.Cell(5).GetString().Trim();
                var cargo = fila.Cell(6).GetString().Trim();
                var fechaIngresoStr = fila.Cell(7).GetString().Trim();
                var fechaCeseStr = fila.Cell(8).GetString().Trim();
                var relacionLaboral = fila.Cell(9).GetString().Trim();
                var detalleRenunciaCompleto = fila.Cell(10).GetString().Trim();
                var detalleRenuncia = fila.Cell(11).GetString().Trim();
                var tiempoMesesStr = fila.Cell(12).GetString().Trim();
                var permanencia = fila.Cell(13).GetString().Trim();

                // Conversión de fechas
                DateTime.TryParse(fechaIngresoStr, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var fechaIngreso);
                DateTime.TryParse(fechaCeseStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaCese);
                DateTime.TryParseExact(mesStr, "MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var mesDate);

                // Conversión de duración
                int.TryParse(tiempoMesesStr, out var tiempoMeses);

                // Validación de campos mínimos requeridos
                if (string.IsNullOrWhiteSpace(unidad) || string.IsNullOrWhiteSpace(numeroPersonal))
                    continue;

                // Creación del objeto
                lista.Add(new RotacionObrero
                {
                    Unidad = unidad,
                    Servicio = servicio,
                    Mes = mesDate != DateTime.MinValue ? mesDate : fechaIngreso,
                    NumeroPersonal = numeroPersonal,
                    ApellidosNombres = apellidosNombres,
                    Cargo = cargo,
                    FechaIngreso = fechaIngreso,
                    FechaCese = fechaCese,
                    RelacionLaboral = relacionLaboral,
                    DetalleRenunciaCompleto = detalleRenunciaCompleto,
                    DetalleRenuncia = detalleRenuncia,
                    TiempoMeses = tiempoMeses,
                    Permanencia = permanencia
                });
            }
        }

        return lista;
    }

    /// <summary>
    ///     Lee un archivo de texto (.txt) que contiene datos separados por comas sobre rotación de obreros
    ///     y devuelve una lista de objetos <see cref="RotacionObrero" />.
    /// </summary>
    /// <param name="ruta">Ruta absoluta al archivo de texto.</param>
    /// <returns>Lista de objetos <see cref="RotacionObrero" /> leídos desde el archivo.</returns>
    public List<RotacionObrero> LeerRotacionObrerosDesdeTxt(string ruta)
    {
        if (!File.Exists(ruta))
        {
            Console.WriteLine($"El archivo no se encontró en la ruta: {ruta}");
            return new List<RotacionObrero>();
        }

        var lineas = File.ReadAllLines(ruta);

        if (!lineas.Any())
        {
            Console.WriteLine("El archivo está vacío.");
            return new List<RotacionObrero>();
        }

        var registros = new List<RotacionObrero>();

        foreach (var linea in lineas)
        {
            if (string.IsNullOrWhiteSpace(linea)) continue;

            var campos = linea.Split(',');

            // Validación de número de campos esperados
            if (campos.Length != 13)
            {
                Console.WriteLine($"Advertencia: La línea no tiene el número esperado de campos. Línea: {linea}");
                continue;
            }

            try
            {
                var rotacionObrero = new RotacionObrero
                {
                    Unidad = campos[0].Trim(),
                    Servicio = campos[1].Trim(),
                    Mes = DateTime.ParseExact(campos[2].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    NumeroPersonal = campos[3].Trim(),
                    ApellidosNombres = campos[4].Trim(),
                    Cargo = campos[5].Trim(),
                    FechaIngreso = DateTime.ParseExact(campos[6].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    FechaCese = DateTime.ParseExact(campos[7].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    RelacionLaboral = campos[8].Trim(),
                    DetalleRenunciaCompleto = campos[9].Trim(),
                    DetalleRenuncia = campos[10].Trim(),
                    TiempoMeses = int.Parse(campos[11].Trim()),
                    Permanencia = campos[12].Trim()
                };

                registros.Add(rotacionObrero);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar la línea: {ex.Message} | Línea: {linea}");
            }
        }

        return registros;
    }
}