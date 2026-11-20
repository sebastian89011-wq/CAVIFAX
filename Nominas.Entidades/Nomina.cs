namespace Nominas.Entidades;

/// <summary>Nómina de un periodo (semana).</summary>
public class Nomina
{
    public int IdNomina { get; set; }
    /// <summary>FK a Usuario que autoriza; null en borrador.</summary>
    public int? IdUsuarioAutoriza { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public EstatusNomina Estatus { get; set; }
    /// <summary>Fecha y hora de autorización; null en borrador.</summary>
    public DateTime? FechaAutorizacion { get; set; }
    /// <summary>Efectivo recibido para cubrir la nómina.</summary>
    public decimal EfectivoRecibido { get; set; }
}
