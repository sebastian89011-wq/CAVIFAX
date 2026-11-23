using Nominas.Entidades;

namespace Nominas.Negocios;

/// <summary>Lo que necesita la página Detalle de una nómina, ya calculado por NominaService.</summary>
public class NominaDetalle
{
    public Nomina Nomina { get; set; } = new();
    public List<DetalleNomina> Detalles { get; set; } = new();
    public List<PagoEmpleado> Pagos { get; set; } = new();

    /// <summary>RN-13: pagos pendientes de semanas anteriores.</summary>
    public List<PagoEmpleado> PendientesAnteriores { get; set; } = new();

    public Dictionary<int, string> NombresEmpleado { get; set; } = new();
    public Dictionary<int, string> NombresObra { get; set; } = new();

    /// <summary>Texto de la semana de cada nómina que aparece en PendientesAnteriores.</summary>
    public Dictionary<int, string> Semanas { get; set; } = new();

    public decimal TotalBruto => Pagos.Sum(p => p.TotalBruto);
    public decimal TotalDescuentos => Pagos.Sum(p => p.TotalDescuentos);
    public decimal TotalNeto => Pagos.Sum(p => p.TotalNeto);
    public decimal TotalPendientesAnteriores => PendientesAnteriores.Sum(p => p.TotalNeto);

    /// <summary>Efectivo que se sugiere pedir al cliente.</summary>
    public decimal EfectivoSugerido => TotalNeto + TotalPendientesAnteriores;
}
