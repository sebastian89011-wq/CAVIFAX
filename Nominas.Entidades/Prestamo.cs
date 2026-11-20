namespace Nominas.Entidades;

/// <summary>Préstamo o anticipo otorgado a un empleado.</summary>
public class Prestamo
{
    public int IdPrestamo { get; set; }
    /// <summary>FK a Empleado.</summary>
    public int IdEmpleado { get; set; }
    public TipoPrestamo Tipo { get; set; }
    public decimal Monto { get; set; }
    public DateOnly Fecha { get; set; }
    /// <summary>Monto que se descuenta cada semana.</summary>
    public decimal AbonoSemanal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public EstatusPrestamo Estatus { get; set; }
}
