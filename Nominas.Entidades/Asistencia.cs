namespace Nominas.Entidades;

/// <summary>Registro diario de asistencia de un empleado.</summary>
public class Asistencia
{
    public int IdAsistencia { get; set; }
    /// <summary>FK a Empleado.</summary>
    public int IdEmpleado { get; set; }
    /// <summary>FK a Obra; null solo si no asistió.</summary>
    public int? IdObra { get; set; }
    public DateOnly Fecha { get; set; }
    public bool Asistio { get; set; }
    public decimal HorasExtra { get; set; }
}
