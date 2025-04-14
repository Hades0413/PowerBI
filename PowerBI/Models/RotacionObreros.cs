using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerBI.Models;

[Table("RotacionObreros")]
public class RotacionObrero
{
    [Key] [Column("Id")] public int Id { get; set; }

    [Column("Unidad")] public string Unidad { get; set; }

    [Column("Servicio")] public string Servicio { get; set; }

    [Column("Mes")] public DateTime Mes { get; set; }

    [Column("NumeroPersonal")] public string NumeroPersonal { get; set; }

    [Column("ApellidosNombres")] public string ApellidosNombres { get; set; }

    [Column("Cargo")] public string Cargo { get; set; }

    [Column("FechaIngreso")] public DateTime FechaIngreso { get; set; }

    [Column("FechaCese")] public DateTime FechaCese { get; set; }

    [Column("RelacionLaboral")] public string RelacionLaboral { get; set; }

    [Column("DetalleRenunciaCompleto")] public string DetalleRenunciaCompleto { get; set; }

    [Column("DetalleRenuncia")] public string DetalleRenuncia { get; set; }

    [Column("TiempoMeses")] public int TiempoMeses { get; set; }

    [Column("Permanencia")] public string Permanencia { get; set; }
}