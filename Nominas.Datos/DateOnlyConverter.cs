using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Nominas.Datos;

/// <summary>
/// El proveedor de Oracle (MySql.EntityFrameworkCore) lee las columnas DATE como DateTime
/// y no sabe convertirlas a DateOnly. Este convertidor traduce en ambos sentidos.
/// </summary>
public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter()
        : base(fecha => fecha.ToDateTime(TimeOnly.MinValue),
               fechaHora => DateOnly.FromDateTime(fechaHora))
    {
    }
}
