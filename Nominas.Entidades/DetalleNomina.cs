namespace Nominas.Entidades;

/// <summary>Costo bruto de un empleado en una obra dentro de una nómina.</summary>
public class DetalleNomina
{
    public int IdDetalle { get; set; }
    /// <summary>FK a Nomina.</summary>
    public int IdNomina { get; set; }
    /// <summary>FK a Empleado.</summary>
    public int IdEmpleado { get; set; }
    /// <summary>FK a Obra.</summary>
    public int IdObra { get; set; }
    public byte DiasTrabajados { get; set; }
    public decimal HorasExtra { get; set; }
    /// <summary>Sueldo diario vigente al calcular la nómina.</summary>
    public decimal SueldoDiarioAplicado { get; set; }
    /// <summary>Pago por hora extra vigente al calcular la nómina.</summary>
    public decimal PagoHoraExtraAplicado { get; set; }
    public decimal Bono { get; set; }
    /// <summary>Costo total antes de descuentos.</summary>
    public decimal CostoBruto { get; set; }
}
