namespace Nominas.Entidades;

/// <summary>Pago neto de un empleado en una nómina.</summary>
public class PagoEmpleado
{
    public int IdPagoEmpleado { get; set; }
    /// <summary>FK a Nomina en la que se generó el pago.</summary>
    public int IdNomina { get; set; }
    /// <summary>FK a Empleado.</summary>
    public int IdEmpleado { get; set; }
    public decimal TotalBruto { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal TotalNeto { get; set; }
    public EstatusPago Estatus { get; set; }
    /// <summary>Fecha de entrega; null si está pendiente.</summary>
    public DateOnly? FechaPago { get; set; }
    /// <summary>FK a Nomina: semana en que se entregó; null si está pendiente.</summary>
    public int? IdNominaPago { get; set; }
}
