namespace Nominas.Entidades;

/// <summary>Trabajador de la empresa.</summary>
public class Empleado
{
    public int IdEmpleado { get; set; }
    /// <summary>FK a Puesto.</summary>
    public int IdPuesto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string? ApellidoMaterno { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string Domicilio { get; set; } = string.Empty;
    public string Curp { get; set; } = string.Empty;
    /// <summary>Número de Seguridad Social.</summary>
    public string Nss { get; set; } = string.Empty;
    /// <summary>Clave de elector de la credencial INE.</summary>
    public string ClaveIne { get; set; } = string.Empty;
    /// <summary>Indica si el empleado entregó toda su documentación.</summary>
    public bool DocumentosCompletos { get; set; }
    public Especialidad? Especialidad { get; set; }
    public TipoContratacion TipoContratacion { get; set; }
    public EstatusEmpleado Estatus { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public decimal SueldoDiario { get; set; }
    /// <summary>Pago por cada hora extra trabajada.</summary>
    public decimal PagoHoraExtra { get; set; }
}
