
namespace Nominas.Entidades;


public class Empleado
{
    public int IdEmpleado { get; set; }
    public int IdPuesto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string? ApellidoMaterno { get; set; } 
    public string Telefono {  get; set; } = string.Empty;
    public string Domicilio {  get; set; } = string.Empty;
    public string Curp { get; set; } = string.Empty;
    public string Nss { get; set; } = string.Empty;
    public string ClaveIne {  get; set; } = string.Empty;
    public bool DocumentosCompletos { get; set; } = false;
    public string? Especialidad {  get; set; } 
    public TipoContratacion TipoContratacion {  get; set; }
    public EstatusEmpleado Estatus { get; set; }
    public DateOnly FechaIngreso {  get; set; }
    public Decimal SueldoDiario {  get; set; }
    public Decimal PagoHora { get; set; }

}