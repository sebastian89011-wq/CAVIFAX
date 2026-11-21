namespace Nominas.Entidades;

/// <summary>Abono a un préstamo descontado en una nómina.</summary>
public class AbonoPrestamo
{
    public int IdAbono { get; set; }
    /// <summary>FK a Prestamo.</summary>
    public int IdPrestamo { get; set; }
    /// <summary>FK a Nomina en la que se descontó.</summary>
    public int IdNomina { get; set; }
    public decimal MontoAbonado { get; set; }
    public DateOnly Fecha { get; set; }
}
