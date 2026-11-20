namespace Nominas.Entidades;

/// <summary>Obra en construcción para un cliente.</summary>
public class Obra
{
    public int IdObra { get; set; }
    /// <summary>FK a Cliente.</summary>
    public int IdCliente { get; set; }
    /// <summary>FK a Empleado: ingeniero responsable.</summary>
    public int IdIngeniero { get; set; }
    /// <summary>FK a Empleado: supervisor de la obra.</summary>
    public int IdSupervisor { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFinProgramada { get; set; }
    /// <summary>Fecha real de terminación; null mientras está en curso.</summary>
    public DateOnly? FechaFinReal { get; set; }
    public decimal PresupuestoManoObra { get; set; }
    /// <summary>Saldo de presupuesto a partir del cual se genera una alerta.</summary>
    public decimal LimiteAlertaSaldo { get; set; }
    /// <summary>Bono que recibe cada trabajador al terminar la obra.</summary>
    public decimal BonoTerminacionPorTrabajador { get; set; }
    public EstatusObra Estatus { get; set; }
}
